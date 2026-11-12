# Класова модель варіанта 11 (RepairService)

Предметна область: заявки на ремонт у сервісному центрі.

## Сутності

| Сутність | Роль | Незмінний | Інваріант |
|---|---|---|---|
| RepairWork | позиція документа (робота) | так, record | Code не порожній; Quantity > 0; UnitPrice >= 0 |
| RepairTicket | документ зі станом | ні, агрегат | Id > 0; CustomerId > 0; DeviceModel не порожній; позиції додаються лише у стані Received |
| Customer | довідник учасників | так, record | Id > 0; FullName не порожнє; Email містить @ |
| ServiceItem | довідник об’єктів обліку (прейскурант) | так, init | Code не порожній; Price >= 0 |

## Стани квитанції

| Зі стану | До стану | Умова | Метод |
|---|---|---|---|
| Received | InWork | є хоча б одна позиція (клієнт погодив вартість) | StartWork |
| Received | Rejected | завжди | Reject |
| InWork | Ready | завжди (ремонт завершено) | Complete |
| InWork | Rejected | завжди (ремонт неможливий) | Reject |
| Ready | Issued | завжди (пристрій видано клієнту) | Issue |

Issued і Rejected — кінцеві стани.