# Architecture

## AutoX Garage Management System

## 1. Overview

AutoX là hệ thống quản lý gara ô tô được xây dựng bằng **.NET và Entity
Framework Core**, áp dụng các nguyên tắc:

-   Clean Architecture
-   Domain Driven Design (DDD)
-   Modular design
-   High performance database design

Mục tiêu kiến trúc:

-   Maintainability (dễ bảo trì)
-   Scalability (dễ mở rộng)
-   Performance (hiệu năng cao)
-   Clear domain boundaries

------------------------------------------------------------------------

## 2. System Architecture

Hệ thống sử dụng **Layered Architecture**.

```plaintext
Client / UI
     │
     ▼
Application Layer
     │
     ▼
Domain Layer
     │
     ▼
Infrastructure Layer
     │
     ▼
Database (SQLite / SQL Server)
```

## Layer Responsibilities

### Presentation Layer

Chịu trách nhiệm:

-   Web API / UI
-   Request handling
-   Response formatting

### Application Layer

Chịu trách nhiệm:

-   Business use cases
-   Service orchestration
-   Transaction coordination

### Domain Layer

Chứa:

-   Domain entities
-   Business rules
-   Domain services

### Infrastructure Layer

Chứa:

-   EF Core DbContext
-   Database configurations
-   External integrations

------------------------------------------------------------------------

## 3. Domain Modules

Hệ thống được chia thành các module chính:

-   Identity
-   Customer
-   Inventory
-   Repair
-   Billing
-   Supplier

Việc chia module giúp:

-   giảm coupling
-   dễ mở rộng
-   dễ maintain

------------------------------------------------------------------------

## 4. Core Entities

Các entity chính trong hệ thống:

```plaintext
Customer
Vehicle
Employee
Supplier
Part
ServiceItem
RepairOrder
RepairTask
RepairOrderItem
Invoice
Transaction
```

------------------------------------------------------------------------

## 5. Entity Relationships

```plaintext
Customer
   │
   ├── Vehicle
   │
   └── Invoice
          │
          ├── RepairOrder
          │       │
          │       ├── RepairTask
          │       │       └── ServiceItem
          │       │
          │       └── RepairOrderItem
          │               └── Part
          │
          └── Transaction
```

------------------------------------------------------------------------

## 6. Module Design

## Identity Module

Entities:

```plaintext
-   Account
-   Employee
```

Chức năng:

-   Authentication
-   Authorization
-   Employee management

------------------------------------------------------------------------

## Customer Module

Entities:

```plaintext
-   Customer
-   Vehicle
```

Relationship:

```plaintext
Customer\
    └── Vehicles
```

Indexes:

-   Email
-   PhoneNumber
-   TaxCode
-   LicensePlate

------------------------------------------------------------------------

## Inventory Module

Entities:

```plaintext
-   Part
-   Supplier
-   SupplierContactPhone
```

Relationship:

```plaintext
Supplier\
    └── Parts
```

Design considerations:

-   Unique PartCode
-   Inventory tracking
-   Manufacturer indexing

------------------------------------------------------------------------

## Repair Module

Đây là **core business module** của gara.

Entities:

```plaintext
-   RepairOrder
-   RepairTask
-   RepairOrderItem
```

Relationship:

```plaintext
RepairOrder\
    ├── RepairTask\
    └── RepairOrderItem
```

------------------------------------------------------------------------

### RepairOrder

Đại diện cho **phiếu sửa xe**.

Properties:

```plaintext
CustomerId\
VehicleId\
InvoiceId\
OrderDate\
CompletionDate\
Status
```

Contains:

```plaintext
Tasks\
Parts
```

------------------------------------------------------------------------

### RepairTask

Đại diện cho **công việc sửa chữa**.

Relationships:

```plaintext
`RepairTask\
├── Employee\
├── ServiceItem\
└── RepairOrder`
```

Responsibilities:

```plaintext
-   Assign technician
-   Track task progress
-   Track repair duration
```

------------------------------------------------------------------------

### RepairOrderItem

Bảng trung gian giữa:

RepairOrder\
Part

Lưu:

PartId\
Quantity

------------------------------------------------------------------------

## 7. Billing Module

Entities:

-   Invoice
-   Transaction
-   ServiceItem

Invoice là **aggregate root của billing module**.

Relationship:

Invoice\
├── RepairOrders\
└── Transactions

------------------------------------------------------------------------

# 8. Financial Calculation Flow

Invoice calculations:

ServiceSubtotal = Sum(ServiceItem.UnitPrice)

PartsSubtotal = Sum(Part.SellingPrice \* Quantity)

Subtotal = ServiceSubtotal + PartsSubtotal

DiscountAmount = Percentage or Fixed

TaxAmount = (Subtotal - Discount) \* TaxRate

TotalAmount = Subtotal - Discount + Tax

BalanceDue = TotalAmount - AmountPaid

------------------------------------------------------------------------

## 9. Data Flow

Luồng nghiệp vụ chính:

Customer arrives\
↓\
Create RepairOrder\
↓\
Add RepairTasks\
↓\
Add Parts\
↓\
Complete Repair\
↓\
Generate Invoice\
↓\
Customer Payment\
↓\
Create Transaction

------------------------------------------------------------------------

## 10. Database Strategy

Primary Key:

Id (Identity)

Foreign Keys:

RepairTask\
- EmployeeId - ServiceItemId - RepairOrderId

RepairOrderItem\
- RepairOrderId - PartId

Transaction\
- InvoiceId

------------------------------------------------------------------------

## 11. Index Strategy

Để tối ưu query:

Customer

-   Email
-   PhoneNumber

Vehicle

-   LicensePlate

Invoice

-   InvoiceNumber

Part

-   PartCode
-   Manufacturer

RepairTask

-   EmployeeId

Transaction

-   InvoiceId
-   Status
-   Type

------------------------------------------------------------------------

## 12. Delete Behavior

Cascade:

RepairOrder → RepairTask\
RepairOrder → RepairOrderItem

Restrict:

Part → Supplier

SetNull:

RepairOrder → Invoice

------------------------------------------------------------------------

## 13. Performance Considerations

Enum được lưu dưới dạng:

byte

Giúp giảm kích thước cột.

Financial fields sử dụng:

decimal(18,2)

Đảm bảo độ chính xác.

------------------------------------------------------------------------

## 14. Future Improvements

Các cải tiến có thể triển khai:

Domain Events

RepairCompletedEvent\
InvoiceGeneratedEvent\
PaymentReceivedEvent

CQRS

Command / Query separation

Background Jobs

Invoice recalculation\
Report generation\
Notifications

------------------------------------------------------------------------

## 15. Summary

Core workflow của hệ thống:

Customer\
→ RepairOrder\
→ RepairTask / Parts\
→ Invoice\
→ Transaction

Kiến trúc được thiết kế để:

-   scalable
-   maintainable
-   high performance
