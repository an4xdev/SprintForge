import type { Sequelize } from "sequelize";
import { Companies as _Companies } from "./Companies";
import type { CompaniesAttributes, CompaniesCreationAttributes } from "./Companies";
import { TaskTypes as _TaskTypes } from "./TaskTypes";
import type { TaskTypesAttributes, TaskTypesCreationAttributes } from "./TaskTypes";

export {
  _Companies as Companies,
  _TaskTypes as TaskTypes,
};

export type {
  CompaniesAttributes,
  CompaniesCreationAttributes,
  TaskTypesAttributes,
  TaskTypesCreationAttributes,
};

export function initModels(sequelize: Sequelize) {
  const Companies = _Companies.initModel(sequelize);
  const TaskTypes = _TaskTypes.initModel(sequelize);


  return {
    Companies: Companies,
    TaskTypes: TaskTypes,
  };
}
