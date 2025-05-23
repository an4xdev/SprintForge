import express from "express";
import { initModels } from "../models/init-models";
const app = express();
const port = process.env.PORT || 6713;

import swaggerUi from 'swagger-ui-express';
import swaggerJSDoc from 'swagger-jsdoc';
import ApiResponse from "../models/ApiResponse";

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

// health check
app.get("/health", async (_req, res) => {
  try {
    await sequelize.authenticate();
    res.status(200).json({
      status: "UP",
      message: "Service is running",
      database: "Connected",
    });
  } catch (error) {
    console.error("Database connection error:", error);
    res.status(500).json({
      status: "DOWN",
      message: "Service is running, but database connection failed",
      database: "Disconnected",
    });
  }
});

// Task Types

app.get("/api/taskTypes", async (_req, res) => {
  const { TaskTypes } = models;
  try {
    const taskTypes = await TaskTypes.findAll();
    if (!taskTypes || taskTypes.length === 0) {
      let response = ApiResponse.NotFound("No task types found");
      res.status(404).json(response);
      return;
    }
    let response = ApiResponse.Success("Task types fetched successfully", taskTypes);
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
    let response = ApiResponse.Created("Task type created successfully", taskType);
    res.status(201).json(response);
  } catch (error) {
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
    let response = ApiResponse.Success("Task type fetched successfully", taskType);
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
      let response = ApiResponse.NotFound("Task type not found");
      res.status(404).json(response);
      return;
    }
    taskType.Name = name;
    await taskType.save();
    let response = ApiResponse.Success("Task type updated successfully", taskType);
    res.status(200).json(response);
  } catch (error) {
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
      let response = ApiResponse.NotFound("Task type not found");
      res.status(404).json(response);
      return;
    }
    await taskType.destroy();
    res.status(204).send();
  } catch (error) {
    console.error("Error deleting task type:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

// Companies

app.get("/api/companies", async (_req, res) => {
  const { Companies } = models;
  try {
    const companies = await Companies.findAll();
    if (!companies || companies.length === 0) {
      let response = ApiResponse.NotFound("No companies found");
      res.status(404).json(response);
      return;
    }

    let response = ApiResponse.Success("Companies fetched successfully", companies);
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
    let response = ApiResponse.Success("Company fetched successfully", company);
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
    let response = ApiResponse.Created("Company created successfully", company);
    res.status(201).json(response);
  } catch (error) {
    console.error("Error creating company:", error);
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
      let response = ApiResponse.NotFound("Company not found");
      res.status(404).json(response);
      return;
    }
    company.Name = name;
    await company.save();
    let response = ApiResponse.Success("Company updated successfully", company);
    res.status(200).json(response);
  } catch (error) {
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
      let response = ApiResponse.NotFound("Company not found");
      res.status(404).json(response);
      return;
    }
    await company.destroy();
    res.status(204).send();
  } catch (error) {
    console.error("Error deleting company:", error);
    let response = ApiResponse.InternalError("Internal server error");
    res.status(500).json(response);
  }
});

sequelize.authenticate()
  .then(() => {
    console.log('Database connection has been established successfully.');
    app.listen(port, () => {
      console.log(`Server is working on port: ${port}`);
    });
  })
  .catch(err => {
    console.error('Unable to connect to the database:', err);
  });