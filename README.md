# Payment Gateway API

A .NET 8 implementation of the payment-gateway take-home exercise. It accepts card payment requests, validates them, sends valid payments to the supplied acquiring bank simulator, stores safe payment details in memory, and supports retrieval by payment ID.

## Prerequisites

- .NET 8 SDK
- Docker Desktop, to run the supplied bank simulator

## API

### Process a payment

```text
POST /api/Payments
```

```json
{
  "cardNumber": "4242424242424241",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 1050,
  "cvv": "123"
}
```

The response returns a GUID and safe payment details only. Card number and CVV are never persisted or returned; only the final four card digits are exposed.

### Retrieve a payment

```text
GET /api/Payments/{id}
```

Returns `200 OK` for a stored payment or `404 Not Found` if it does not exist.

### Invalid payment information

Invalid requests return `400 Bad Request` with a response status of `Rejected`. They are not forwarded to the bank and are not saved.

Validation rules include:

- card number: 14 - 19 numeric characters;
- CVV: 3 - 4 numeric characters;
- expiry month/year: a future month;
- currency: GBP, USD, or EUR; (made use of the most popular)
- amount: a positive integer.

## Tests

The tests are unit tests and do not need Docker. They cover validation, repository storage, payment service outcomes, and bank error mapping, along with some minor exception handling checks.

## Design decisions

- **Thin controller, service-led workflow:** the controller owns HTTP concerns; `PaymentsService` coordinates validation, the bank call, safe payment creation, and persistence.
- **Explicit boundaries:** `IBankClient` isolates the acquiring-bank HTTP contract and `IPaymentsRepository` isolates storage. Both can be replaced in tests without running the simulator.
- **In-memory persistence:** a thread-safe `ConcurrentDictionary` is appropriate for the exercise, which does not require a database. Data is intentionally lost on restart.
- **Sensitive-data handling:** full PAN and CVV are used only while forwarding a valid request to the bank. The stored `Payment` has only the final four card digits.
- **Validation outcomes:** malformed input is represented as `Rejected`, returns `400`, is neither sent to the bank nor stored. Valid input can only be `Authorized` or `Declined` after the bank response.
- **Failure responses:** a global exception handler converts simulator unavailability to `503` and other bank communication errors to `502`, using `ProblemDetails` JSON.

## Project structure

```text
src/PaymentGateway.Api/
  Clients/            # acquiring-bank boundary
  Contracts/          # HTTP and simulator request/response DTOs
  Controllers/        # HTTP endpoints
  Domain/             # safe stored payment model and status enum
  ExceptionHandling/  # global ProblemDetails handler
  Repositories/       # in-memory payment storage
  Services/           # payment-processing workflow
  Validations/        # FluentValidation rules
test/PaymentGateway.Api.Tests/
  ...                 # focused unit tests
imposters/            # supplied bank-simulator configuration
```
