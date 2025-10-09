import express from "express";
import { initModels } from "../models/init-models";
const app = express();
const port = process.env.PORT || 6713;

import swaggerUi from 'swagger-ui-express';
import swaggerJSDoc from 'swagger-jsdoc';
import ApiResponse from "../models/ApiResponse";
import { auditService } from "./auditService";

app.use(express.json());

const options = {
  definition: {
    openapi: '3.0.0',
    info: {
      title: 'JS Express Service API',
      version: '1.0.0',
      description: 'Documentation for JS Express Service API',
    },
    servers: [
      {
        url: 'http://localhost:6713',
        description: 'Development server',
      }
    ]
  },
  apis: ['./*.js']
};

const swaggerSpec = swaggerJSDoc(options);

app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));

import { Sequelize } from "sequelize";

const requiredEnvVars = ['DB_DATABASE', 'DB_USERNAME', 'DB_PASSWORD', 'DB_HOST', 'DB_PORT'];

for (const varName of requiredEnvVars) {
  if (!process.env[varName]) {
    console.error(`${varName} environment variable is required`);
    process.exit(1);
  }
}

const sequelize = new Sequelize(
  process.env.DB_DATABASE!,
  process.env.DB_USERNAME!,
  process.env.DB_PASSWORD!,
  {
    host: process.env.DB_HOST!,
    port: Number(process.env.DB_PORT!),
    dialect: "postgres",
    logging: false,
  }
);

const models = initModels(sequelize);

app.get("/health", async (_req, res) => {
  let dbStatus = "Connected";
  let rabbitStatus = "Connected";
  let overallStatus = "UP";
  let statusCode = 200;

  try {
    await sequelize.authenticate();
  } catch (error) {
    console.error("Database connection error:", error);
    dbStatus = "Disconnected";
    overallStatus = "DOWN";
    statusCode = 503;
  }

  try {
    if (!auditService) {
      throw new Error("Audit service not initialized");
    }
  } catch (error) {
    console.error("RabbitMQ connection error:", error);
    rabbitStatus = "Disconnected";
    overallStatus = "DOWN";
    statusCode = 503;
  }

  res.status(statusCode).json({
    status: overallStatus,
    message: "Service is running",
    database: dbStatus,
    rabbitmq: rabbitStatus,
  });
});

// Task Types

app.get("/api/taskTypes", async (_req, res) => {
  const { TaskTypes } = models;
  try {
    const taskTypes = await TaskTypes.findAll();
    if (!taskTypes || taskTypes.length === 0) {
      let response = ApiResponse.Success("No task types found", []);
      res.status(200).json(response);
      return;
    }

    const transformedTaskTypes = taskTypes.map(taskType => ({
      id: taskType.Id,
      name: taskType.Name
    }));

    let response = ApiResponse.Success("Task types fetched successfully", transformedTaskTypes);
    res.status(200).json(response);
  } catch (error) {
    console.error("Error fetching task types:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.post("/api/taskTypes", async (req, res) => {
  const { TaskTypes } = models;
  const { name } = req.body;
  try {
    const taskType = await TaskTypes.create({
      Name: name
    });

    const transformedTaskType = {
      id: taskType.Id,
      name: taskType.Name
    };

    await auditService.logAction("CREATE_SUCCESS", "TaskType", `Created new task type: ${name}`);

    let response = ApiResponse.Created("Task type created successfully", transformedTaskType);
    res.status(201).json(response);
  } catch (error) {
    await auditService.logAction("CREATE_FAILED", "TaskType", `Error creating task type: ${name}`);
    console.error("Error creating task type:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.get("/api/taskTypes/:id", async (req, res) => {
  const { TaskTypes } = models;
  const { id } = req.params;
  try {
    const taskType = await TaskTypes.findByPk(id);
    if (!taskType) {
      let response = ApiResponse.NotFound("Task type not found");
      res.status(404).json(response);
      return;
    }

    const transformedTaskType = {
      id: taskType.Id,
      name: taskType.Name
    };

    let response = ApiResponse.Success("Task type fetched successfully", transformedTaskType);
    res.status(200).json(response);
  } catch (error) {
    console.error("Error fetching task type:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.put("/api/taskTypes/:id", async (req, res) => {
  const { TaskTypes } = models;
  const { id } = req.params;
  const { name } = req.body;
  try {
    const taskType = await TaskTypes.findByPk(id);
    if (!taskType) {
      await auditService.logAction("UPDATE_FAILED", "TaskType", `Task type not found: ${id}`);
      let response = ApiResponse.BadRequest("Task type not found");
      res.status(400).json(response);
      return;
    }
    taskType.Name = name;
    await taskType.save();

    const transformedTaskType = {
      id: taskType.Id,
      name: taskType.Name
    };

    await auditService.logAction("UPDATE_SUCCESS", "TaskType", `Updated task type: ${name}`);
    let response = ApiResponse.Success("Task type updated successfully", transformedTaskType);
    res.status(200).json(response);
  } catch (error) {
    await auditService.logAction("UPDATE_FAILED", "TaskType", `Error updating task type: ${id}`);
    console.error("Error updating task type:", error);
    res.status(500).json({ error: "Internal server error" });
  }
});

app.delete("/api/taskTypes/:id", async (req, res) => {
  const { TaskTypes } = models;
  const { id } = req.params;
  try {
    const taskType = await TaskTypes.findByPk(id);
    if (!taskType) {
      await auditService.logAction("DELETE_FAILED", "TaskType", `Task type not found: ${id}`);
      let response = ApiResponse.NotFound("Task type not found");
      res.status(404).json(response);
      return;
    }
    await taskType.destroy();
    await auditService.logAction("DELETE_SUCCESS", "TaskType", `Deleted task type: ${taskType.Name}`);
    res.status(204).send();
  } catch (error) {
    console.error("Error deleting task type:", error);
    let response = ApiResponse.InternalError("Internal server error");
    await auditService.logAction("DELETE_FAILED", "TaskType", `Error deleting task type: ${id}`);
    res.status(500).json(response);
  }
});

// Companies

app.get("/api/companies/count", async (_req, res) => {
  const { Companies } = models;
  try {
    const count = await Companies.count();
    let response = ApiResponse.Success("Companies count fetched successfully", count);
    res.status(200).json(response);
  } catch (error) {
    console.error("Error fetching companies count:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.get("/api/companies", async (_req, res) => {
  const { Companies } = models;
  try {
    const companies = await Companies.findAll();
    if (!companies || companies.length === 0) {
      let response = ApiResponse.Success("No companies found", []);
      res.status(200).json(response);
      return;
    }

    const transformedCompanies = companies.map(company => ({
      id: company.Id,
      name: company.Name
    }));

    let response = ApiResponse.Success("Companies fetched successfully", transformedCompanies);
    res.status(200).json(response);
  } catch (error) {
    console.error("Error fetching companies:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.get("/api/companies/:id", async (req, res) => {
  const { Companies } = models;
  const { id } = req.params;
  try {
    const company = await Companies.findByPk(id);
    if (!company) {
      let response = ApiResponse.NotFound("Company not found");
      res.status(404).json(response);
      return;
    }

    const transformedCompany = {
      id: company.Id,
      name: company.Name
    };

    let response = ApiResponse.Success("Company fetched successfully", transformedCompany);
    res.status(200).json(response);
  } catch (error) {
    console.error("Error fetching company:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.post("/api/companies", async (req, res) => {
  const { Companies } = models;
  const { name } = req.body;
  try {
    const company = await Companies.create({
      Name: name
    });

    const transformedCompany = {
      id: company.Id,
      name: company.Name
    };

    await auditService.logAction("CREATE_SUCCESS", "Company", `Created new company: ${name}`);

    let response = ApiResponse.Created("Company created successfully", transformedCompany);
    res.status(201).json(response);
  } catch (error) {
    console.error("Error creating company:", error);
    await auditService.logAction("CREATE_FAILED", "Company", `Error creating company: ${name}`);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.put("/api/companies/:id", async (req, res) => {
  const { Companies } = models;
  const { id } = req.params;
  const { name } = req.body;
  try {
    const company = await Companies.findByPk(id);
    if (!company) {
      await auditService.logAction("UPDATE_FAILED", "Company", `Company not found: ${id}`);
      let response = ApiResponse.BadRequest("Company not found");
      res.status(400).json(response);
      return;
    }
    if (company.Name === "Default") {
      await auditService.logAction("UPDATE_FAILED", "Company", `Attempted to modify default company: ${id}`);
      let response = ApiResponse.BadRequest("Default company cannot be modified");
      res.status(400).json(response);
      return;
    }
    company.Name = name;
    await company.save();

    await auditService.logAction("UPDATE_SUCCESS", "Company", `Updated company: ${name}`);

    const transformedCompany = {
      id: company.Id,
      name: company.Name
    };

    let response = ApiResponse.Success("Company updated successfully", transformedCompany);
    res.status(200).json(response);
  } catch (error) {
    await auditService.logAction("UPDATE_FAILED", "Company", `Error updating company: ${id}`);
    console.error("Error updating company:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

app.delete("/api/companies/:id", async (req, res) => {
  const { Companies } = models;
  const { id } = req.params;
  try {
    const company = await Companies.findByPk(id);
    if (!company) {
      await auditService.logAction("DELETE_FAILED", "Company", `Company not found: ${id}`);
      let response = ApiResponse.NotFound("Company not found");
      res.status(404).json(response);
      return;
    }
    if (company.Name === "Default") {
      await auditService.logAction("DELETE_FAILED", "Company", `Attempted to delete default company: ${id}`);
      let response = ApiResponse.BadRequest("Default company cannot be deleted");
      res.status(400).json(response);
      return;
    }
    await company.destroy();
    await auditService.logAction("DELETE_SUCCESS", "Company", `Deleted company: ${company.Name}`);
    res.status(204).send();
  } catch (error) {
    console.error("Error deleting company:", error);
    await auditService.logAction("DELETE_FAILED", "Company", `Error deleting company: ${id}`);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

Promise.all([
  sequelize.authenticate(),
  auditService.initialize()
])
  .then(() => {
    console.log('Database connection has been established successfully.');
    app.listen(port, () => {
      console.log(`Server is working on port: ${port}`);
    });
  })
  .catch(err => {
    console.error('Unable to connect to the database or initialize audit service:', err);
  });