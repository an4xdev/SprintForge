import * as Sequelize from 'sequelize';
import { DataTypes, Model, Optional } from 'sequelize';

export interface TaskTypesAttributes {
  Id: number;
  Name: string;
}

export type TaskTypesPk = "Id";
export type TaskTypesId = TaskTypes[TaskTypesPk];
export type TaskTypesCreationAttributes = Optional<TaskTypesAttributes, 'Id'>;

export class TaskTypes extends Model<TaskTypesAttributes, TaskTypesCreationAttributes> implements TaskTypesAttributes {
  Id!: number;
  Name!: string;


  static initModel(sequelize: Sequelize.Sequelize): typeof TaskTypes {
    return TaskTypes.init({
      Id: {
        autoIncrement: true,
        autoIncrementIdentity: true,
        type: DataTypes.INTEGER,
        allowNull: false,
        primaryKey: true
      },
      Name: {
        type: DataTypes.TEXT,
        allowNull: false
      }
    }, {
      sequelize,
      tableName: 'TaskTypes',
      schema: 'public',
      timestamps: false,
      indexes: [
        {
          name: "PK_TaskTypes",
          unique: true,
          fields: [
            { name: "Id" },
          ]
        },
      ]
    });
  }
}
