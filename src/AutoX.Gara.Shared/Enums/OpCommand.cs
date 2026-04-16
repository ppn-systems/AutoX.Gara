using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

public enum OpCommand : System.UInt16
{
    NONE = 0x00,
    PING = 0x0200,
    HANDSHAKE = 0x0201,
    LOGIN = 0x0210,
    LOGOUT = 0x0211,
    REGISTER = 0x0212,
    CHANGE_PASSWORD = 0x0213,

    CUSTOMER_GET = 0x100,

    CUSTOMER_CREATE = 0x101,

    CUSTOMER_UPDATE = 0x102,

    CUSTOMER_DELETE = 0x103,

    VEHICLE_GET = 0x150,

    VEHICLE_CREATE = 0x151,

    VEHICLE_UPDATE = 0x152,

    VEHICLE_DELETE = 0x153,

    SUPPLIER_GET = 0x160,

    SUPPLIER_CREATE = 0x161,

    SUPPLIER_UPDATE = 0x162,

    SUPPLIER_DELETE = 0x163,

    SUPPLIER_CHANGE_STATUS = 0x164,

    PART_GET = 0x180,

    PART_CREATE = 0x181,

    PART_UPDATE = 0x182,

    PART_DELETE = 0x183,

    INVOICE_GET = 0x1A0,

    INVOICE_CREATE = 0x1A1,

    INVOICE_UPDATE = 0x1A2,

    INVOICE_DELETE = 0x1A3,

    REPAIR_ORDER_GET = 0x1B0,

    REPAIR_ORDER_CREATE = 0x1B1,

    REPAIR_ORDER_UPDATE = 0x1B2,

    REPAIR_ORDER_DELETE = 0x1B3,

    TRANSACTION_GET = 0x1C0,

    TRANSACTION_CREATE = 0x1C1,

    TRANSACTION_UPDATE = 0x1C2,

    TRANSACTION_DELETE = 0x1C3,

    SERVICE_ITEM_GET = 0x1D0,

    SERVICE_ITEM_CREATE = 0x1D1,

    SERVICE_ITEM_UPDATE = 0x1D2,

    SERVICE_ITEM_DELETE = 0x1D3,

    REPAIR_TASK_GET = 0x1E0,

    REPAIR_TASK_CREATE = 0x1E1,

    REPAIR_TASK_UPDATE = 0x1E2,

    REPAIR_TASK_DELETE = 0x1E3,

    REPAIR_ORDER_ITEM_GET = 0x1F0,

    REPAIR_ORDER_ITEM_CREATE = 0x1F1,

    REPAIR_ORDER_ITEM_UPDATE = 0x1F2,

    REPAIR_ORDER_ITEM_DELETE = 0x1F3,

    /// <summary>
    /// Get list of employees.
    /// </summary>
    EMPLOYEE_GET = 6001,

    /// <summary>
    /// Create a new employee.
    /// </summary>
    EMPLOYEE_CREATE = 6002,

    /// <summary>
    /// Update an existing employee.
    /// </summary>
    EMPLOYEE_UPDATE = 6003,

    /// <summary>
    /// Change employee status.
    /// </summary>
    EMPLOYEE_CHANGE_STATUS = 6004,

    /// <summary>
    /// Get list of employee salaries.
    /// </summary>
    EMPLOYEE_SALARY_GET = 6010,

    /// <summary>
    /// Create a new employee salary record.
    /// </summary>
    EMPLOYEE_SALARY_CREATE = 6011,

    /// <summary>
    /// Update an employee salary record.
    /// </summary>
    EMPLOYEE_SALARY_UPDATE = 6012,

    /// <summary>
    /// Delete an employee salary record.
    /// </summary>
    EMPLOYEE_SALARY_DELETE = 6013,
}