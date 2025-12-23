## HW4 Асинхронное межсервисное взаимодействие
Данный проект представляет собой микросервисную систему интернет-магазина «Гозон»,  
реализующую асинхронное взаимодействие между сервисами заказов и оплат  
с гарантией доставки сообщений и семантикой **effectively exactly once**

Система построена на микросервисной архитектуре с использованием **API Gateway**,  
**Orders Service**, **Payments Service** и брокера сообщений

## Возможности системы

### Payments Service
- Создание счета пользователя (не более одного счета на пользователя)
- Пополнение счета
- Просмотр текущего баланса

### Orders Service
- Создание заказа
- Асинхронный запуск процесса оплаты заказа
- Просмотр списка заказов
- Просмотр статуса конкретного заказа

### API Gateway
- Единая точка входа в систему
- Routing HTTP-запросов к микросервисам
- Единый Swagger UI для тестирования всех API

## Технологии

- ASP.NET Core 8
- PostgreSQL
- RabbitMQ
- YARP Reverse Proxy (API Gateway)
- Docker / Docker Compose
- Swagger

## Асинхронное взаимодействие

Процесс создания заказа и оплаты реализован асинхронно через очередь сообщений.

Используемые паттерны:
- **Transactional Outbox** - в Orders Service
- **Transactional Inbox + Outbox** - в Payments Service
- Идемпотентная обработка сообщений
- Семантика **effectively exactly once** при списании средств

## Пользовательский сценарий создания заказа

1. Клиент отправляет запрос на создание заказа  
2. Orders Service:
   - создает заказ в БД
   - сохраняет задачу оплаты в Outbox
3. Orders Service публикует событие оплаты в очередь
4. Payments Service:
   - получает событие
   - проверяет наличие счета пользователя
   - списывает средства (идемпотентно)
   - формирует событие об успешной / неуспешной оплате
5. Payments Service отправляет событие в очередь
6. Orders Service:
   - получает событие
   - обновляет статус заказа
   - 
### Статусы заказа
- `0` - заказ создан
- `1` - оплата прошла успешно
- `2` - оплата не прошла
- 
## API Gateway

Gateway реализован как **Reverse Proxy** и отвечает **только за routing**
### Swagger UI (единый)
```http://localhost:5000/gw-swagger```

### Маршрутизация
- `/api/orders/**` → Orders Service
- `/api/payments/**` → Payments Service

Прямой доступ к микросервисам извне отключен
## Основные API

### Payments Service
- `POST /api/payments/accounts` - создать счёт
- `POST /api/payments/accounts/{userId}/top-up` - пополнить счёт
- `GET  /api/payments/accounts/{userId}` - получить баланс

### Orders Service
- `POST /api/orders` - создать заказ
- `GET  /api/orders` - получить список заказов
- `GET  /api/orders/{id}` - получить статус заказа

## Docker

Проект разворачивается с помощью `docker-compose.yml` и включает:

- Orders Service (PostgreSQL)
- Payments Service (PostgreSQL)
- RabbitMQ
- API Gateway

Все сервисы контейнеризованы.  
Микросервисы **не публикуют свои порты наружу**, доступ возможен только через Gateway

## Запуск проекта
```bash
git clone https://github.com/evelonk/HW4CSE
cd HW4CSE
docker compose up --build
