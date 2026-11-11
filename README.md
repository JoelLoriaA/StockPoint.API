# StockPoint.API

Back-end Web API for a point-of-sale system, built with ASP.NET Core (.NET 10) as a team project for a university course (Universidad de Costa Rica, IF4101). It provides product management, read-only customer and employee lookups, and work-order registration with stock updates.

The desktop client that consumes this API lives in a separate repository: [StockPoint-WPF](https://github.com/AdrianBarbozaV/StockPoint-WPF) (WPF).

This is a portfolio copy of a team project. The commit history preserves each author's work.

## Architecture

Four projects, organized in three layers:

| Project | Layer | Responsibility |
|---|---|---|
| `StockPoint.API` | REST controllers | HTTP endpoints, request DTOs, error mapping |
| `StockPoint.Business` | Business | Validation, price and tax calculation, business exceptions |
| `StockPoint.Data` | Data | ADO.NET access to SQL Server (`Microsoft.Data.SqlClient`), explicit SQL transactions |
| `StockPoint.Domain` | Domain | Plain domain classes shared by the other layers |

Requests flow `Controller -> Business -> Data`.

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/productos` | List products |
| GET | `/api/productos/{id}` | Get a product |
| GET | `/api/productos/buscar?termino=` | Search products by name or barcode |
| GET | `/api/productos/{id}/ordenes` | Orders in which a product was sold |
| POST | `/api/productos` | Create a product |
| PUT | `/api/productos/{id}` | Update a product |
| DELETE | `/api/productos/{id}` | Delete a product |
| GET | `/api/clientes`, `/api/clientes/{id}` | List / get customers (read-only) |
| GET | `/api/empleados`, `/api/empleados/{id}` | List / get employees (read-only) |
| POST | `/api/ordenes` | Register a work order |
| GET | `/api/ordenes/{numero}` | Get an order with its lines |

## Behavior

- **Orders:** the Business layer validates the request, takes the unit price from the product catalog, computes each line subtotal (price x quantity - discount), the VAT per line from the product's tax rate, and the order totals. The Data layer then saves the header, the lines and the stock decrement in a single SQL transaction.
- **Barcodes** are generated automatically when a product is created.
- **Deleting** a product that is referenced by orders is rejected with a clear message.
- **Errors:** validation problems return `400`, missing entities `404`, insufficient stock `409`. Any unhandled exception returns a generic `500` JSON body; details go to the server log only.

## Configuration

The connection string is read from `ConnectionStrings:StockPointDB`. The values committed in `StockPoint.API/appsettings.json` are placeholders (connection details present in earlier history were replaced with placeholders in this copy). `StockPoint.API/appsettings.example.json` shows the expected format.

Provide real values without committing them, for example with an environment variable (PowerShell):

```powershell
$env:ConnectionStrings__StockPointDB = "Data Source=<server>;Initial Catalog=<database>;User ID=<user>;Password=<password>;Encrypt=False"
```

The SQL Server database schema is not included in this repository.

## Running

```bash
dotnet run --project StockPoint.API
```

Launch profiles listen on `http://localhost:5212` and `https://localhost:7116`. The OpenAPI document is mapped in the Development environment.

## Testing

`StockPoint.API/StockPoint.API.http` contains requests for every endpoint, including error cases (not found, invalid input, insufficient stock). There is no automated test project.

## Team and attribution

| Member | Role in this repository |
|---|---|
| Jared | Project skeleton, Domain model classes, Data layer and the SQL Server connection setup |
| Joel Loría Arcia | Business layer, REST controllers, request DTOs and the `.http` test file; also contributions to the Data layer (`OrdenData`, `ProductoData`), `Program.cs` and the `OrdenPorProducto` domain class |
| Adrián Barboza | WPF client (separate repository); no commits in this repository |
| Emanuel Araya | WPF client, product management view (separate repository); no commits in this repository |

In [StockPoint-WPF](https://github.com/AdrianBarbozaV/StockPoint-WPF), Joel Loría Arcia's contributions are the order lookup view and the integration with this API (HTTP services, JSON model mapping, endpoint fixes and error handling).

## License

No license is currently specified for this repository.
