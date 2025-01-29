

## API de Produtos - .NET Core com Redis e Banco de Dados em Memória
Esta aplicação API RESTful foi construída com .NET Core e implementa operações CRUD (Criar, Ler, Atualizar e Deletar) para gerenciamento de produtos. Para otimizar as consultas de leitura, foi integrado o uso de Redis para as operações GET de busca de produtos. A aplicação utiliza um banco de dados em memória para armazenar os produtos durante a execução.

### Tecnologias Utilizadas
- .NET Core 8
- Redis
- Entity Framework Core (InMemory Database)
- ASP.NET Core Web API

### Funcionalidades
- GET /products - Retorna todos os produtos.
- GET /products/{id} - Retorna um produto específico pelo ID.
- POST /products - Cria um novo produto.
- PUT /products/{id} - Atualiza um produto existente.
- DELETE /products/{id} - Deleta um produto.
  
### Pré-requisitos
- .NET SDK: Você precisará do SDK do .NET Core instalado.
- Redis: Certifique-se de ter o Redis em execução na sua máquina ou em um servidor acessível. Se não tiver o Redis instalado, pode usá-lo via Docker ou em um serviço de nuvem como o Redis Labs.

### Instalação
1. Clonando o Repositório
Clone este repositório para sua máquina local:

``` bash

https://github.com/gabriela-ncmt/ProductApiWithRedis.git
```
2. Configuração do Redis
Certifique-se de que o Redis esteja em funcionamento. Se for local, o Redis geralmente está disponível no endereço localhost:6379 por padrão.

Se for necessário configurar o Redis em Docker, use:

``` bash

docker run --name redis -p 6380:6380 -d redis
```

3. Configuração do Banco de Dados em Memória
A aplicação utiliza o banco de dados em memória do Entity Framework Core. Não há necessidade de configurar um banco de dados tradicional, pois os dados serão armazenados na memória enquanto a aplicação estiver em execução.

Assegure-se de que a aplicação está configurada para usar o banco de dados em memória. O código de configuração padrão para isso está no arquivo  Program.cs:
``` csharp


builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseInMemoryDatabase("ProductsDb"));

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")));
```

4. Executando a Aplicação
Com tudo configurado, execute a aplicação:

``` bash

dotnet run
```
Isso irá iniciar a API no endereço http://localhost:7020.

### Endpoints
1. GET /products: 
Recupera todos os produtos armazenados no banco de dados em memória. Caso um produto tenha sido consultado recentemente, a resposta será retornada do Redis (se disponível).

2. GET /products/{id}: 
Recupera um produto específico pelo ID. A consulta pode ser feita diretamente no banco de dados em memória ou em Redis se o produto foi consultado recentemente.

3. POST /products: 
Cria um novo produto.

4. PUT /products/{id}: 
Atualiza um produto existente pelo ID.

5. DELETE /products/{id}: 
Deleta um produto existente pelo ID.


### Arquitetura e Fluxo de Dados
GET /products: O endpoint verifica primeiro se os dados estão armazenados no Redis. Caso contrário, ele consulta o banco de dados em memória e armazena os resultados no Redis para otimizar futuras consultas.

GET /products/{id}: O produto é consultado no Redis. Caso não esteja presente, o sistema faz uma consulta no banco de dados em memória e o armazena no Redis para otimização.

POST /products, PUT /products/{id} e DELETE /products/{id}: Esses endpoints não dependem do Redis diretamente, mas podem ser configurados para invalidar ou atualizar o cache do Redis após alterações no banco de dados em memória.
