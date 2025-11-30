
# Clean Architecture - Apuntes de Curso

Este documento recopila y estructura los conceptos clave sobre Clean Architecture, basándose en el curso de _Dometrain_. Se centra en la separación de responsabilidades, la independencia de la base de datos y la mantenibilidad del software.

## 1. Evolución de los Patrones de Arquitectura

Antes de llegar a Clean Architecture, es importante entender de dónde venimos y qué problemas intentamos resolver. La arquitectura de software ha evolucionado significativamente a lo largo de las décadas, buscando soluciones a los desafíos de escalabilidad, mantenibilidad y complejidad creciente de los sistemas.

### Principales Patrones de Arquitectura

#### **N-Tier / Layered Architecture (Arquitectura por Capas)**
La arquitectura clásica por capas fue durante mucho tiempo el estándar de facto. Organiza el código en capas horizontales (Presentación, Lógica de Negocio, Acceso a Datos), donde cada capa solo puede comunicarse con la capa inmediatamente inferior. Aunque simple de entender, tiene limitaciones importantes que veremos más adelante.

```mermaid
graph TD
    UI[Capa de Presentación<br/>UI / Controllers] --> BL[Capa de Lógica de Negocio<br/>Business Logic]
    BL --> DAL[Capa de Acceso a Datos<br/>Data Access Layer]
    DAL --> DB[(Base de Datos)]
    
    style UI fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style BL fill:#6b7280,stroke:#374151,stroke-width:2px
    style DAL fill:#5a6471,stroke:#2d3439,stroke-width:2px
    style DB fill:#4b5563,stroke:#1f2937,stroke-width:2px
```

**Ventajas:** Separación clara de responsabilidades, fácil de entender.  
**Desventajas:** Dependencias rígidas hacia abajo, la base de datos se convierte en el centro, difícil de testear.

#### **Microservices Architecture (Arquitectura de Microservicios)**
Divide la aplicación en servicios pequeños, independientes y desplegables por separado. Cada microservicio posee su propia base de datos y se comunica con otros a través de APIs (REST, gRPC, mensajería).

```mermaid
graph TD
    Client[Cliente] --> API[API Gateway]
    API --> MS1[Microservicio 1<br/>User Service]
    API --> MS2[Microservicio 2<br/>Order Service]
    API --> MS3[Microservicio 3<br/>Payment Service]
    
    MS1 --> DB1[(DB 1)]
    MS2 --> DB2[(DB 2)]
    MS3 --> DB3[(DB 3)]
    
    MS1 -.Message Bus.-> MS2
    MS2 -.Message Bus.-> MS3
    
    style Client fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style API fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style MS1 fill:#6b7280,stroke:#374151,stroke-width:2px
    style MS2 fill:#6b7280,stroke:#374151,stroke-width:2px
    style MS3 fill:#6b7280,stroke:#374151,stroke-width:2px
    style DB1 fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style DB2 fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style DB3 fill:#4b5563,stroke:#1f2937,stroke-width:2px
```

**Ventajas:** Escalabilidad independiente, tecnologías heterogéneas, equipos autónomos.  
**Desventajas:** Complejidad operacional, transacciones distribuidas, consistencia eventual.

#### **Service-Oriented Architecture (SOA)**
Precursor de los microservicios, SOA organiza la funcionalidad en servicios reutilizables que se comunican mediante protocolos estándar (SOAP, XML). A diferencia de los microservicios, SOA tiende a tener servicios más grandes y mayor acoplamiento a través de un Enterprise Service Bus (ESB).

```mermaid
graph TD
    App1[Aplicación 1] --> ESB[Enterprise Service Bus<br/>ESB]
    App2[Aplicación 2] --> ESB
    App3[Aplicación 3] --> ESB
    
    ESB --> S1[Servicio de<br/>Autenticación]
    ESB --> S2[Servicio de<br/>Facturación]
    ESB --> S3[Servicio de<br/>Inventario]
    
    S1 --> DB[(Bases de Datos<br/>Compartidas)]
    S2 --> DB
    S3 --> DB
    
    style App1 fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style App2 fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style App3 fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style ESB fill:#6b7280,stroke:#374151,stroke-width:3px
    style S1 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style S2 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style S3 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style DB fill:#4b5563,stroke:#1f2937,stroke-width:2px
```

**Ventajas:** Reutilización de servicios, integración empresarial.  
**Desventajas:** ESB como punto único de fallo, complejidad en la orquestación.

#### **Modular Monolith (Monolito Modular)**
Un enfoque híbrido que mantiene todos los módulos en un solo proceso (monolito) pero con límites claros y bien definidos entre módulos. Es un paso intermedio hacia los microservicios sin su complejidad operacional.

```mermaid
graph TD
    subgraph Monolith[Aplicación Monolítica]
        M1[Módulo Users<br/>API bien definida]
        M2[Módulo Orders<br/>API bien definida]
        M3[Módulo Payments<br/>API bien definida]
        M4[Módulo Notifications<br/>API bien definida]
    end
    
    M1 -.Interfaz.-> M2
    M2 -.Interfaz.-> M3
    M3 -.Interfaz.-> M4
    
    Monolith --> DB[(Base de Datos<br/>Compartida)]
    
    style Monolith fill:#5a6471,stroke:#2d3439,stroke-width:3px
    style M1 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style M2 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style M3 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style M4 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style DB fill:#4b5563,stroke:#1f2937,stroke-width:2px
```

**Ventajas:** Simplicidad de despliegue, transacciones ACID, refactorización más sencilla.  
**Desventajas:** Escalado vertical, posible degradación hacia un "big ball of mud" si no se mantiene la disciplina.

#### **Event Driven Architecture (Arquitectura Basada en Eventos)**
Los componentes se comunican mediante eventos asíncronos. Un productor publica eventos y los consumidores reaccionan a ellos sin conocerse mutuamente. Muy útil en sistemas reactivos y de alta concurrencia.

```mermaid
graph LR
    P1[Producer 1] -->|Publica Evento| EB[Event Bus<br/>RabbitMQ/Kafka]
    P2[Producer 2] -->|Publica Evento| EB
    
    EB -->|Suscripción| C1[Consumer 1]
    EB -->|Suscripción| C2[Consumer 2]
    EB -->|Suscripción| C3[Consumer 3]
    
    C1 --> DB1[(DB 1)]
    C2 --> DB2[(DB 2)]
    C3 --> DB3[(DB 3)]
    
    style P1 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style P2 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style EB fill:#6b7280,stroke:#374151,stroke-width:3px
    style C1 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style C2 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style C3 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style DB1 fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style DB2 fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style DB3 fill:#4b5563,stroke:#1f2937,stroke-width:2px
```

**Ventajas:** Desacoplamiento temporal, escalabilidad, resiliencia.  
**Desventajas:** Debugging complejo, eventual consistency, gestión de sagas.

#### **MV Patterns (MVC, MVP, MVVM)**
Patrones de presentación que separan la lógica de interfaz de usuario de la lógica de negocio:

-   **MVC (Model-View-Controller):** El controlador maneja la entrada del usuario, actualiza el modelo y selecciona la vista.
-   **MVP (Model-View-Presenter):** El presentador actúa como intermediario entre vista y modelo, haciendo la vista más pasiva.
-   **MVVM (Model-View-ViewModel):** El ViewModel expone datos del modelo en un formato que la vista puede enlazar fácilmente (data binding).

```mermaid
graph TD
    subgraph MVC[Patrón MVC]
        User1[Usuario] --> Controller[Controller]
        Controller --> Model1[Model]
        Controller --> View1[View]
        View1 --> User1
        Model1 -.Notifica.-> View1
    end

    style Controller fill:#6b7280,stroke:#374151,stroke-width:2px
    style Model1 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style View1 fill:#8b95a3,stroke:#5a6471,stroke-width:2px
```
```mermaid
graph TD
    subgraph MVP[Patrón MVP]
        User2[Usuario] --> View2[View<br/>Pasiva]
        View2 --> Presenter[Presenter]
        Presenter --> Model2[Model]
        Presenter --> View2
    end
    
    style Presenter fill:#6b7280,stroke:#374151,stroke-width:2px
    style Model2 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style View2 fill:#8b95a3,stroke:#5a6471,stroke-width:2px
```
```mermaid
graph TD
    subgraph MVVM[Patrón MVVM]
        User3[Usuario] --> View3[View]
        View3 <-->|Data Binding| ViewModel[ViewModel]
        ViewModel --> Model3[Model]
    end
    
    style ViewModel fill:#6b7280,stroke:#374151,stroke-width:2px
    style Model3 fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style View3 fill:#8b95a3,stroke:#5a6471,stroke-width:2px
```

Estos patrones son complementarios a las arquitecturas de sistema y se enfocan específicamente en la capa de presentación.

### El problema de la Arquitectura por Capas (N-Tier)

En la arquitectura tradicional de 3 capas (Tier 1, 2, 3), las dependencias fluyen hacia abajo, generalmente terminando en la base de datos.

```mermaid
graph TD
    User((Cliente)) --> Presentation[Capa Presentación]
    Presentation --> Logic[Lógica de Negocio]
    Logic --> Data[Acceso a Datos / DB]
    
    style Data fill:#6b7280,stroke:#374151,stroke-width:2px

```

**El gran inconveniente:** Todas las flechas acaban apuntando a la Base de Datos. Con el tiempo, esto provoca que el acceso a datos se mezcle con la lógica de negocio e incluso con la presentación. Los **DATOS** se convierten en el centro de la arquitectura, en lugar de serlo la **LÓGICA DE NEGOCIO**.

## 2. Arquitecturas Centradas en el Dominio

El objetivo de las arquitecturas modernas es invertir las dependencias. Queremos que la **Lógica de Negocio (Dominio)** sea el núcleo y no dependa de nada externo (ni UI, ni BD, ni Frameworks).

Variantes principales:

-   **Clean Architecture** (Robert C. Martin)
    
-   Hexagonal Architecture (Ports and Adapters)
    
-   Onion Architecture
    

### Estructura de Clean Architecture

Las capas interiores definen interfaces (el "qué") y las capas exteriores definen la implementación (el "cómo").

```mermaid
graph TD
    subgraph Infrastructure [Capa Infraestructura]
        DB[(Base de Datos)]
        Ext[Servicios Externos]
    end

    subgraph Presentation [Capa Presentación]
        UI[API / Web / Consola]
    end

    subgraph Core [Núcleo de la Aplicación]
        Application[Capa Aplicación]
        Domain[Capa Dominio]
    end

    %% Dependencias (Corregido: Conectar Nodos en vez de Subgraphs)
    UI --> Application
    DB --> Application
    Ext --> Application
    Application --> Domain
    DB --> Domain
    
    %% Estilos
    style Application fill:#6b7280,stroke:#374151,stroke-width:2px
    style Domain fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style Infrastructure fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style Presentation fill:#7c8794,stroke:#4b5563,stroke-width:2px
```

> **Regla de Dependencia:** El código fuente solo puede apuntar hacia adentro. El Dominio no sabe nada de la Infraestructura.

## 3. Desglose de Capas

Este diagrama ilustra la relación entre las capas lógicas y la estructura física de proyectos recomendada en el curso (Sección 0020), destacando la separación de los contratos.


## 🏗️ Clean Architecture (Arquitectura Limpia)

Este diagrama representa la jerarquía y el flujo de dependencias de una aplicación bajo los principios de Arquitectura Limpia, donde las flechas apuntan **hacia dentro**, hacia las capas más estables y de alto nivel (el Dominio). 

[Image of the Clean Architecture Onion Diagram]

```mermaid
graph TD
    subgraph Presentation [Capa Presentación]
        Contracts["Contracts Project<br>(Requests, Responses)"]
        API["API Project<br>(Program.cs, Controllers)"]
    end

    subgraph Application [Capa Aplicación]
        App["Application Project"]
    end

    subgraph Domain [Capa Dominio]
        Dom["Domain Project"]
    end

    subgraph Infrastructure [Capa Infraestructura]
        Infra["Infrastructure Project"]
    end

    %% Dependencias entre proyectos
    API --> Contracts
    App --> Dom
    API --> App
    Infra --> App

    style Presentation fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style Application fill:#6b7280,stroke:#374151,stroke-width:2px
    style Domain fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style Infrastructure fill:#8b95a3,stroke:#5a6471,stroke-width:2px
```

### A. Capa de Presentación (Presentation Layer)

Es el punto de entrada al sistema.

-   **Responsabilidades:** Interactuar con el exterior, visualizar datos, traducir datos (DTOs) y administrar la configuración del framework (ej. ASP.NET Core).
    
-   **Estructura:** Se recomienda separar los contratos (Requests/Responses) en un proyecto o librería aparte.
    

**¿Por qué separar los contratos?** Permite empaquetar `Contracts` (ej. un paquete NuGet) para que los clientes (Frontend, otros microservicios) puedan consumirlos sin depender de la API completa.

**Ejemplo de Estructura:**

-   `GymManagement.Api` (Proyecto WebAPI)
    
-   `GymManagement.Contracts` (Class Library - Solo DTOs)
    

### B. Capa de Aplicación (Application Layer)

Contiene la lógica de los casos de uso. Orquesta el flujo de información.

-   **Responsabilidades:**
    
    1.  Ejecutar casos de uso.
        
    2.  Obtener objetos del Dominio (Fetch).
        
    3.  Manipular objetos del Dominio (Lógica de negocio pura).
        
    4.  Guardar cambios (Persistencia).
        

### C. Capa de Infraestructura (Infrastructure Layer)

Se encarga de todo lo que es "detalle de implementación" y comunicación con el mundo físico.

-   Base de datos (EF Core, Dapper).
    
-   Servicios externos (Stripe, SendGrid).
    
-   Sistema de archivos, Reloj del sistema, etc.
    

## 4. Inyección de Dependencias (DI) en Clean Arch

Existe un desafío: La API (Presentación) necesita inyectar servicios definidos en `Application` pero implementados en `Infrastructure`. Sin embargo, para mantener la arquitectura limpia, no deberíamos acoplar fuertemente la API a la Infraestructura.

**Solución: Extension Methods para DI** Cada capa es responsable de definir su propia inyección de dependencias.

```mermaid
graph TD
    subgraph API["API / Program.cs<br/>(Composition Root)"]
        Program["Program.cs<br/>builder.Services<br/>.AddApplication()<br/>.AddInfrastructure()"]
    end
    
    subgraph AppLayer["Application Layer"]
        AppDI["DependencyInjection.cs<br/>AddApplication()"]
        IService["ISubscriptionService<br/>(Interfaz)"]
        AppDI -.Registra.-> IService
    end
    
    subgraph InfraLayer["Infrastructure Layer"]
        InfraDI["DependencyInjection.cs<br/>AddInfrastructure()"]
        IRepo["ISubscriptionRepository<br/>(Interfaz)"]
        RepoImpl["SubscriptionRepository<br/>(Implementación)"]
        InfraDI -.Registra.-> IRepo
        InfraDI -.Registra.-> RepoImpl
    end
    
    subgraph DomainLayer["Domain Layer"]
        Entities["Entidades<br/>Subscription, Gym"]
    end
    
    Program -->|Llama| AppDI
    Program -->|Llama| InfraDI
    
    IService -->|Depende de| IRepo
    RepoImpl -.Implementa.-> IRepo
    IRepo -->|Usa| Entities
    
    style API fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style AppLayer fill:#6b7280,stroke:#374151,stroke-width:2px
    style InfraLayer fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style DomainLayer fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style Program fill:#9ca5b3,stroke:#6a7481,stroke-width:2px
    style AppDI fill:#7b8694,stroke:#4b5a67,stroke-width:2px
    style InfraDI fill:#8c96a4,stroke:#5b6572,stroke-width:2px
```

> **Principio clave:** La API (Composition Root) es el único lugar que conoce todas las capas. Cada capa registra sus propias dependencias a través de Extension Methods, manteniendo la separación de responsabilidades.

#### 1. En la capa de Aplicación (`DependencyInjection.cs`)

```
// Proyecto: Application
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar servicios propios de la capa de aplicación
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        
        // Registrar MediatR u otros validadores aquí
        return services;
    }
}

```

#### 2. En la capa de Infraestructura (`DependencyInjection.cs`)

```
// Proyecto: Infrastructure
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Aquí es donde la implementación concreta se une a la interfaz
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IEmailSender, SendGridEmailSender>();
        
        return services;
    }
}

```

#### 3. En la API (`Program.cs`)

El punto de entrada es el único lugar donde se permite "conocer" todas las capas para ensamblarlas.

```
var builder = WebApplication.CreateBuilder(args);

// Una sola línea por capa mantiene el Program.cs limpio
builder.Services
    .AddApplication()
    .AddInfrastructure(); 

var app = builder.Build();
app.Run();

```

#### A tener en cuenta:

> **¡OJO!** Se podría pensar que se viola el principio de responsabilidades únicas de las capas, ya que la capa de ```INFRAESTRUCTURA``` debe ser referenciada por la capa de ```PRESENTACIÓN```, pero esto se hace únicamente por la inyección de dependencias. Se debe evitar a toda cosa utilizar cualquier otra referencia de la capa de infraestructura en la capa de presentación. Es por ello que se suele optar por soluciones como declarar pública la clase ```DependencyInjection``` e internal cualquier otra.

## 5. Patrones Tácticos

### CQRS (Command Query Responsibility Segregation)

Separa las operaciones de lectura de las de escritura.

-   **Commands:** Escriben datos. Mutan el estado. Generalmente devuelven `void` o el ID del recurso creado.
    
-   **Queries:** Leen datos. No mutan el estado. Devuelven datos (DTOs).

```mermaid
graph LR
    subgraph Write["Lado de Escritura (Commands)"]
        CMD[CreateCommand] --> CH[Command Handler]
        CH --> WM[Write Model]
        WM --> WDB[(Base de Datos<br/>Escritura)]
    end
    
    subgraph Read["Lado de Lectura (Queries)"]
        QRY[GetQuery] --> QH[Query Handler]
        QH --> RM[Read Model]
        RM --> RDB[(Base de Datos<br/>Lectura)]
    end
    
    Client[Cliente] -->|Escribe| CMD
    Client -->|Lee| QRY
    
    WDB -.Sincronización.-> RDB
    
    style Write fill:#6b7280,stroke:#374151,stroke-width:2px
    style Read fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style CMD fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style QRY fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style CH fill:#7b8694,stroke:#4b5a67,stroke-width:2px
    style QH fill:#8c96a4,stroke:#5b6572,stroke-width:2px
    style WDB fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style RDB fill:#4b5563,stroke:#1f2937,stroke-width:2px
```
    

> **Nota:** No confundir con CQS (Command Query Separation) que es a nivel de método. CQRS es a nivel de arquitectura/objetos.

### Patrón Mediator

Desacopla los controladores de los servicios. El controlador envía un mensaje y un "mediador" busca quién debe manejarlo.

-   Favorece: **Slice by Feature** (organizar carpetas por funcionalidad: `CreateSubscription`, `GetSubscription`) en lugar de por tipo técnico (`Controllers`, `Services`).
    

```mermaid
sequenceDiagram
    participant API as Controller
    participant M as Mediator
    participant H as Handler
    participant D as Domain/DB

    API->>M: Send(CreateSubscriptionCommand)
    M->>H: Localiza y ejecuta Handler
    H->>D: Ejecuta lógica de negocio
    D-->>H: Retorna resultado
    H-->>M: Retorna respuesta
    M-->>API: Retorna respuesta

```

### Patrón Result

Evitar el uso de Excepciones para el flujo de control lógico. Las excepciones son para errores inesperados. Para validaciones o reglas de negocio, usamos un objeto `Result` (o bibliotecas como `ErrorOr`).

```mermaid
graph TD
    Start[Handler ejecuta lógica] --> Validate{Validación<br/>exitosa?}
    
    Validate -->|Sí| Success["Return Success<br/>ErrorOr&lt;Guid&gt;(subscriptionId)"]
    Validate -->|No| Error["Return Error<br/>ErrorOr&lt;Guid&gt;(Errors.User.HasActiveSubscription)"]
    
    Success --> Controller[Controller recibe Result]
    Error --> Controller
    
    Controller --> Match{"Result.Match()"}
    
    Match -->|Success| OK["Ok(value)<br/>HTTP 200"]
    Match -->|Error| Problem["Problem(errors)<br/>HTTP 4xx"]
    
    style Start fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style Validate fill:#6b7280,stroke:#374151,stroke-width:2px
    style Success fill:#5a8a5a,stroke:#2d5a2d,stroke-width:2px
    style Error fill:#8a5a5a,stroke:#5a2d2d,stroke-width:2px
    style Controller fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style Match fill:#6b7280,stroke:#374151,stroke-width:2px
    style OK fill:#6a8a6a,stroke:#3d5a3d,stroke-width:2px
    style Problem fill:#8a6a6a,stroke:#5a3d3d,stroke-width:2px
```

**Implementación conceptual:**

```
// En lugar de devolver int o lanzar excepción:
public async Task<ErrorOr<Guid>> Handle(...) 
{
    if (user.Subscriptions.Count > 0)
    {
        // Retornamos un objeto de error controlado
        return Errors.User.HasActiveSubscription;
    }
    
    // ... lógica ...
    return subscription.Id;
}

// En el Controller:
var result = await _mediator.Send(command);

return result.Match(
    value => Ok(value),
    errors => Problem(errors) // Mapea el error a un código HTTP correcto (409, 404, etc.)
);

```

### Repository Pattern

Abstracción entre el Dominio/Aplicación y la Base de Datos.

-   **Interfaz (`IRepository`):** Vive en la capa de **Dominio** (o Aplicación).
    
-   **Implementación (`Repository`):** Vive en la capa de **Infraestructura**.
    
-   **Ventajas:** Facilita el Unit Testing (podemos mockear la BD) y permite cambiar la tecnología de persistencia sin tocar la lógica de negocio.

```mermaid
graph TD
    subgraph Application["Application Layer"]
        Service[Subscription Service]
        IRepo["&lt;&lt;Interface&gt;&gt;<br/>ISubscriptionRepository<br/>+ GetById()<br/>+ Add()<br/>+ Update()<br/>+ Delete()"]
    end
    
    subgraph Infrastructure["Infrastructure Layer"]
        Repo["SubscriptionRepository<br/>(Implementación con EF Core)"]
        RepoAlt["SubscriptionRepository<br/>(Implementación con Dapper)"]:::alternative
    end
    
    subgraph Domain["Domain Layer"]
        Entity[Subscription Entity]
    end
    
    Service -->|Depende de| IRepo
    Repo -.Implementa.-> IRepo
    RepoAlt -.Implementa.-> IRepo
    
    Repo --> DB1[(Base de Datos<br/>SQL Server)]
    RepoAlt --> DB2[(Base de Datos<br/>PostgreSQL)]:::alternative
    
    IRepo -->|Retorna| Entity
    Repo -->|Retorna| Entity
    
    style Application fill:#6b7280,stroke:#374151,stroke-width:2px
    style Infrastructure fill:#7c8794,stroke:#4b5563,stroke-width:2px
    style Domain fill:#4b5563,stroke:#1f2937,stroke-width:2px
    style Service fill:#7b8694,stroke:#4b5a67,stroke-width:2px
    style IRepo fill:#8b95a3,stroke:#5a6471,stroke-width:2px
    style Repo fill:#8c96a4,stroke:#5b6572,stroke-width:2px
    style Entity fill:#5a6471,stroke:#2d3439,stroke-width:2px
    style DB1 fill:#4b5563,stroke:#1f2937,stroke-width:2px
    
    classDef alternative fill:#6b7280,stroke:#374151,stroke-width:1px,stroke-dasharray: 5 5
```
    

### Unit of Work Pattern

Este patrón actúa como un "director de orquesta" para las transacciones de base de datos, resolviendo problemas de integridad cuando una operación de negocio involucra cambios en múltiples agregados o repositorios.

**El Problema: Inconsistencia Parcial**
Imagina el escenario: **"Crear una suscripción y actualizar el estado del gimnasio"**. Sin Unit of Work, si las operaciones guardan cambios individualmente:

1.  `_subscriptionRepo.Add(sub)` $\rightarrow$ **Éxito (Guardado en BD)**.
2.  `_gymRepo.Update(gym)` $\rightarrow$ **Fallo (Error de red/SQL)**.

**Resultado:** Tienes una suscripción huérfana para un gimnasio que no ha registrado el cambio. El sistema ha quedado en un estado inconsistente.

**La Solución: Atomicidad**
`UnitOfWork` asegura que todos los cambios se traten como una única transacción atómica (**ACID**).

  * **Todo tiene éxito:** Se persisten todos los cambios a la vez.
  * **Algo falla:** No se guarda nada (Rollback automático).

<!-- end list -->

```mermaid
sequenceDiagram
    participant Handler
    participant RepoSub as Subscription Repo
    participant RepoGym as Gym Repo
    participant UoW as UnitOfWork
    participant DB as Base de Datos

    Note over Handler, DB: Fase 1: Preparación (En Memoria)
    Handler->>RepoSub: AddAsync(sub)
    RepoSub-->>Handler: (Tracking State: Added)
    
    Handler->>RepoGym: UpdateAsync(gym)
    RepoGym-->>Handler: (Tracking State: Modified)

    Note over DB: La BD sigue intacta
    
    Note over Handler, DB: Fase 2: Persistencia (Atómica)
    Handler->>UoW: CommitChangesAsync()
    UoW->>DB: BEGIN TRANSACTION
    UoW->>DB: INSERT Subscription + UPDATE Gym
    alt Éxito
        DB-->>UoW: COMMIT
    else Error
        DB-->>UoW: ROLLBACK (Nada se guarda)
    end
```

**Ejemplo de Implementación Robusta:**

En este patrón, los repositorios **no llaman a `SaveChanges`**. Solo marcan las entidades como "agregadas", "modificadas" o "eliminadas" en el contexto compartido.

```csharp
public async Task<ErrorOr<Guid>> Handle(CreateSubscriptionCommand command)
{
    // 1. Lógica de Dominio y Validaciones
    var gym = await _gymRepo.GetByIdAsync(command.GymId);
    if (gym is null) return Errors.Gym.NotFound;

    var sub = new Subscription(command.UserId, command.PlanId);

    // Actualizamos el agregado Gym (Lógica en memoria)
    var result = gym.AddSubscription(sub);
    if (result.IsError) return result.Errors;

    try 
    {
        // 2. Preparación de la persistencia
        // Estos métodos NO impactan la BD, solo avisan al Contexto
        await _subscriptionRepo.AddAsync(sub);
        await _gymRepo.UpdateAsync(gym);

        // 3. Commit (Punto de no retorno)
        // Se abre la transacción, se envían los cambios y se cierra.
        await _unitOfWork.CommitChangesAsync();

        return sub.Id;
    }
    catch (Exception)
    {
        // Si falla el Commit, nada se guardó. La integridad se mantiene.
        // Aquí podríamos loguear el error.
        return Errors.Subscription.CommitFailed;
    }
}
```

> **Nota de Arquitectura:** La interfaz `IUnitOfWork` se define en la capa de **Aplicación** (o Dominio), mientras que su implementación concreta (que inyecta el `DbContext` de EF Core) reside en la capa de **Infraestructura**, respetando la Regla de Dependencia.
>

## 6. Capa de infraestructura

### Responsabilidades

- Interactuar con la solución persistente. Ej: bases de datos SQL/NoSQL.
- Interactuar con otros servicios externos como servicios de email, pasarelas de pago, message brokers, etc.
- Interactuar con el sistema operativo (sistema de archivos, reloj del sistema, etc).
