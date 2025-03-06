# Globant - Challenge Técnico BCP 👨🏼‍💻

>El siguiente repositorio contiene la solucion al problema de AntiFraud utilizando microservicios construidos en .NET 8, Kafka y PostgreSQL, además de generar una comunicación entre microservicios Dockerizando las soluciones.

## 🚀 Prerequisitos

### 🛠 **1. Instalar y Configurar PostgreSQL**

#### 📌 **Opción 1: Usar Docker**

En Docker, se puede ejecutar PostgreSQL que ya está configurado en `docker-compose.yml`:

```yaml
version: "3.7"
services:
  postgres:
    image: postgres:14
    container_name: postgres_db
    restart: always
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: rquilcat #la contraseña es de ejemplo, usado en mi cuenta local
      POSTGRES_DB: transactions_db
    volumes:
      - postgres_data:/var/lib/postgresql/data
```

#### 📌 **Opción 2: Instalar PostgreSQL Localmente**

1. Descargar PostgreSQL desde [aquí](https://www.postgresql.org/download/).  
2. Después de instalar y configurar PostgreSQL, crear una base de datos llamada `transactions_db`.  
3. Usar `pgAdmin` o `psql` para ejecutar:

   ```sql
   CREATE DATABASE transactions_db;
   ```

---

### 🛠 **2. Instalar y Configurar Kafka**

#### 📌 **Ejecutar Kafka con Docker**

Kafka y Zookeeper son necesarios. En `docker-compose.yml` ya está configurado para levantarlos:

```yaml
version: "3.7"
services:
  zookeeper:
    image: confluentinc/cp-zookeeper:5.5.3
    container_name: zookeeper
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181

  kafka:
    image: confluentinc/cp-kafka:5.5.3
    container_name: kafka
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_ZOOKEEPER_CONNECT: "zookeeper:2181"
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_BROKER_ID: 1
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_JMX_PORT: 9991
```

Ejecutar:

```sh
docker-compose up -d
```

Para verificar que Kafka funciona:

```sh
docker ps
```

---

## 🗄️ Configuracion Inicial

### 🛠 En el entorno de .NET

#### 📌 **1. Configurar `appsettings.json` en Transaction Service**

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=postgres;Database=transactions_db;Username=postgres;Password=rquilcat"
  },
  "Kafka": {
    "BootstrapServers": "kafka:29092",
    "TransactionCreatedTopic": "transaction_created",
    "TransactionUpdatedTopic": "transaction_updated"
  }
}
```

#### 📌 **2. Configurar `appsettings.json` en AntiFraud Service**

```json
{
  "Kafka": {
    "BootstrapServers": "kafka:29092",
    "GroupId": "antifraud-group",
    "TransactionTopic": "transaction-topic",
    "TransactionCreatedTopic": "transaction_created",
    "TransactionUpdatedTopic": "transaction_updated"
  }
}
```

#### 📌 **3. Aplicar Migraciones de Entity Framework**

Ejecutar estos comandos:

```sh
dotnet ef migrations add InitialCreate --project Infrastructure
dotnet ef database update --project Infrastructure
```

Y verificamos en la base de datos en PostgreSQL:

```sh
psql -U postgres -d transactions_db -c "SELECT * FROM \"Transactions\";"
```

---

## 🗄️ Ejecucion del Proyecto

### 📌 **1. Crear una transacción** (`POST http://localhost:5000/api/transaction`)

**Body:**

```json
{
  "sourceAccountId": "550e8400-e29b-41d4-a716-446655440000",
  "targetAccountId": "550e8400-e29b-41d4-a716-446655440001",
  "transferTypeId": 1,
  "value": 1500
}
```

✅ **Si el valor es mayor a 2000 o el total diario supera 20000, será rechazado por AntiFraudService.**

### 📌 **2. Consultar una transacción** (`GET http://localhost:5001/api/antifraud/check/{transactionId}`)

Ejemplo:

```text
POST http://localhost:5001/api/antifraud/check/{transactionId}?value=120
```

### 📌 **3. Verificar Kafka**

Puedes usar `kafkacat` para verificar los mensajes:

```sh
kafkacat -b localhost:9092 -t transactions -C -o beginning
```

---

## 📁 Arquitecutra de la solucion

La arquitectura utilizada para la solución del proyecto es **Arquitectura Hexagonal** y podemos verla representada de la siguiente manera

📌 **Dominio (Domain/)** → Contiene la lógica de negocio pura y los modelos.
📌 **Aplicación (Application/)** → Contiene los servicios de aplicación (casos de uso) y orquestación.
📌 **Infraestructura (Infrastructure/)** → Implementa detalles técnicos (Kafka, PostgreSQL, repositorios).
📌 **Adaptadores de Entrada (WebApi/)** → Controladores que exponen las APIs REST.
📌 **Adaptadores de Salida (Infrastructure/Kafka/ y Infrastructure/Persistence/)** → Implementaciones de repositorios y comunicación con Kafka.

También se utilizo **el principio de DDD** para representar eventos dentro de la capa de Dominio, para tener en claro que tipo de operaciones vamos a realizar.

En cuanto a la comunicación de los componentes de la arquitectura, en base a los microservicios es el siguiente:

```text
+------------------+        +------------------+
|  TransactionService  | ⇄ | PostgreSQL DB     |
+------------------+        +------------------+
         |                            ↑
         | Kafka: "transaction_created"
         ↓
+------------------+
|  Kafka Broker    |
+------------------+
         |
         | Kafka: "transaction_created"
         ↓
+------------------+
|  AntiFraudService |
+------------------+
         |
         | Kafka: "transaction_validated"
         ↓
+------------------+
|  TransactionService (Update Status) |
+------------------+
```

---

Solution Developed by: **Rodrigo Miguel Quilcat Pesantes**
