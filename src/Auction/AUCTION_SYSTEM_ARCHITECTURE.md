# Sistema de Leilão - Arquitetura Detalhada

## 📋 Visão Geral do Sistema

Sistema de leilão online de alta performance projetado para suportar **3.000 usuários simultâneos por leilão** utilizando:

- **Domain-Driven Design (DDD)**
- **Clean Architecture**
- **Event-Driven Architecture**
- **CQRS** (Command Query Responsibility Segregation)
- **PostgreSQL** (Write Database)
- **Redis** (Cache & Read Database)
- **Kafka** (Event Streaming)
- **SignalR** (Real-time Notifications)

### Problema Principal: Concorrência

O maior desafio é gerenciar **race conditions** quando múltiplos usuários tentam dar lances simultaneamente no mesmo leilão. A solução implementa:

- ✅ **Processamento Assíncrono** via Kafka
- ✅ **Sequence Numbers Atômicos** via Redis INCR
- ✅ **Optimistic Locking** no PostgreSQL
- ✅ **Particionamento por Leilão** no Kafka
- ✅ **Cache Distribuído** para leitura rápida

---

## 🏗️ Arquitetura de Alto Nível

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                              │
│  (Web App, Mobile App, SignalR Real-time Updates)              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                           │
│  (API Gateway, REST/GraphQL Controllers, SignalR Hubs)         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     APPLICATION LAYER                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │   Commands   │  │   Queries    │  │   Handlers   │         │
│  │  (Write)     │  │   (Read)     │  │   (Logic)    │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       DOMAIN LAYER                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │  Aggregates  │  │    Entities  │  │Value Objects │         │
│  │  - Auction   │  │    - Bid     │  │   - Money    │         │
│  │  - User      │  │  - Category  │  │   - Email    │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │  PostgreSQL  │  │    Redis     │  │    Kafka     │         │
│  │   (Write)    │  │   (Cache)    │  │  (Events)    │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Domain Layer - Estrutura Completa

### 1. Bounded Contexts

#### **Auction Context**
- Gestão de leilões (criação, início, fim, cancelamento)
- Regras de negócio do leilão
- Estados e transições

#### **Bidding Context**
- Processamento de lances
- Validação de lances
- Lance automático (Proxy Bid)
- Histórico de lances

#### **User Context**
- Gestão de usuários (PF e PJ)
- Autenticação e autorização
- Perfis e preferências

#### **Payment Context** (Futuro)
- Processamento de pagamentos
- Integração com gateways
- Gestão de transações

---

### 2. Aggregates (Agregados)

#### **Auction Aggregate Root**

```csharp
Auction (Aggregate Root)
├── AuctionId (Value Object)
├── Title (string)
├── Description (string)
├── StartingPrice (Money Value Object)
├── ReservePrice (Money Value Object) - preço mínimo para efetivação da venda
├── BuyNowPrice (Money Value Object) - compra imediata (opcional)
├── CurrentPrice (Money Value Object) - preço atual do maior lance
├── BidIncrement (Money Value Object) - incremento mínimo entre lances
├── StartDate (DateTime)
├── EndDate (DateTime)
├── Status (AuctionStatus Enum)
├── CurrentWinningBidId (BidId Value Object)
├── SellerId (UserId Value Object)
├── WinnerId (UserId Value Object)
├── Category (Category Entity)
├── AuctionRules (Value Object)
└── TotalBids (int)
```

**Responsabilidades:**
- ✅ Validar regras de criação do leilão
- ✅ Controlar ciclo de vida (Draft → Active → Ended)
- ✅ Registrar lance vencedor (após validação)
- ✅ Aplicar extensão automática de tempo
- ✅ Emitir eventos de domínio

**Invariantes:**
- EndDate > StartDate
- ReservePrice >= StartingPrice
- BidIncrement > 0
- Só aceita registros de lances se Status = Active
- Não pode ser modificado após início

**Por que NÃO gerencia lances diretamente?**
- Evitar contenção de lock em alta concorrência
- Permite processamento paralelo de lances
- Separação de responsabilidades

---

#### **Bid Aggregate Root**

```csharp
Bid (Aggregate Root)
├── BidId (Value Object)
├── AuctionId (Value Object)
├── BidderId (UserId Value Object)
├── Amount (Money Value Object)
├── BidTime (DateTime) - precisão de milissegundos
├── Status (BidStatus Enum)
├── Version (long) - Optimistic Concurrency Control
├── SequenceNumber (long) - ordem sequencial global dos lances
└── ProxyBid (ProxyBid Value Object) - lance automático
```

**Por que Agregado Separado?**
- ⚡ **Performance**: Não bloqueia o leilão
- 📈 **Escalabilidade**: Processamento paralelo
- 🔄 **Event Sourcing**: Histórico completo
- ⏱️ **Validação Assíncrona**: Aceita e valida depois

**Estados do Lance (BidStatus):**
```
Pending → Accepted → Winning → Won
         ↓          ↓
      Rejected   Outbid → Lost
```

---

#### **User Aggregate Root**

```csharp
User (Abstract Base)
├── UserId (Value Object)
├── Email (Value Object)
├── Password (Value Object)
└── CreatedAt (DateTime)

IndividualUser : User
├── Cpf (Value Object)
└── [métodos específicos de PF]

CorporateUser : User
├── Cnpj (Value Object)
└── CompanyName (string)
```

**Estratégia:** Table Per Hierarchy (TPH) no EF Core

---

### 3. Value Objects

```csharp
// Auction.Domain/ValueObjects/

Money
├── Amount (decimal)
└── Currency (string)

AuctionId
└── Value (Guid)

BidId
└── Value (Guid)

UserId
└── Value (Guid)

Email
└── Value (string)

Cpf
└── Value (string)

Cnpj
└── Value (string)

Password
├── Hash (string)
└── Salt (string)

ProxyBid
├── MaxAmount (Money)
├── CurrentAmount (Money)
└── IsActive (bool)

AuctionRules
├── ExtensionTime (TimeSpan) - tempo adicional se houver lance próximo ao fim
├── ExtensionWindow (TimeSpan) - janela de tempo para extensão (ex: 5 min)
├── MaxBidsPerUser (int)
└── AllowProxyBids (bool)
```

**Características dos Value Objects:**
- ✅ Imutáveis (`record` types)
- ✅ Validação no construtor
- ✅ Retornam `Result<T>` no factory method
- ✅ Não têm identidade própria

---

### 4. Enumerations

```csharp
// Auction.Domain/Enums/

public enum AuctionStatus
{
    Draft = 0,           // Rascunho (não visível)
    Scheduled = 1,       // Agendado para início
    Active = 2,          // Ativo (recebendo lances)
    Paused = 3,          // Pausado temporariamente
    Ended = 4,           // Finalizado (sem vencedor se não atingiu reserve)
    Cancelled = 5,       // Cancelado
    AwaitingPayment = 6, // Aguardando pagamento do vencedor
    Completed = 7        // Completo (pago e entregue)
}

public enum BidStatus
{
    Pending = 0,    // Aguardando validação
    Accepted = 1,   // Aceito como lance válido
    Rejected = 2,   // Rejeitado (valor inválido, leilão encerrado, etc.)
    Outbid = 3,     // Superado por outro lance
    Winning = 4,    // Lance vencedor atual
    Won = 5,        // Venceu o leilão
    Lost = 6        // Perdeu o leilão
}

public enum AuctionType
{
    Standard = 0,  // Leilão inglês (preço ascendente)
    Dutch = 1,     // Leilão holandês (preço descendente)
    Sealed = 2,    // Lances selados (sem visibilidade)
    Reverse = 3    // Leilão reverso (menor preço ganha)
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
```

---

### 5. Domain Events

```csharp
// Auction.Domain/Events/

// Auction Events
public record AuctionCreatedEvent(Guid AuctionId) : IDomainEvent;

public record AuctionStartedEvent(Guid AuctionId) : IDomainEvent;

public record AuctionEndedEvent(
    Guid AuctionId, 
    UserId? WinnerId, 
    bool HasWinner) : IDomainEvent;

public record AuctionCancelledEvent(
    Guid AuctionId, 
    string Reason) : IDomainEvent;

public record AuctionExtendedEvent(
    Guid AuctionId, 
    DateTime NewEndDate) : IDomainEvent;

// Bid Events
public record BidPlacedEvent(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    DateTime BidTime) : IDomainEvent;

public record BidAcceptedEvent(
    Guid BidId,
    Guid AuctionId,
    decimal Amount) : IDomainEvent;

public record BidRejectedEvent(
    Guid BidId,
    Guid AuctionId,
    string Reason) : IDomainEvent;

public record BidOutbidEvent(
    Guid BidId,
    Guid BidderId) : IDomainEvent;

public record ProxyBidTriggeredEvent(
    Guid BidId,
    Guid AuctionId,
    decimal NewAmount) : IDomainEvent;

// User Events
public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Document) : IDomainEvent;
```

---

### 6. Entities - Implementação Detalhada

#### **Auction Entity**

```csharp
// Auction.Domain/Entities/Auction.cs

public class Auction : BaseEntity
{
    protected Auction() { }
    
    private Auction(
        string title,
        string description,
        Money startingPrice,
        Money reservePrice,
        Money bidIncrement,
        DateTime startDate,
        DateTime endDate,
        UserId sellerId,
        Category category,
        AuctionRules rules) : base(Guid.NewGuid(), DateTime.UtcNow, null)
    {
        Title = title;
        Description = description;
        StartingPrice = startingPrice;
        ReservePrice = reservePrice;
        BidIncrement = bidIncrement;
        StartDate = startDate;
        EndDate = endDate;
        SellerId = sellerId;
        Category = category;
        Rules = rules;
        Status = AuctionStatus.Draft;
        CurrentPrice = startingPrice;
        TotalBids = 0;
    }
    
    // Properties
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Money StartingPrice { get; private set; }
    public Money ReservePrice { get; private set; }
    public Money? BuyNowPrice { get; private set; }
    public Money CurrentPrice { get; private set; }
    public Money BidIncrement { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public AuctionStatus Status { get; private set; }
    public UserId SellerId { get; private set; }
    public UserId? WinnerId { get; private set; }
    public Category Category { get; private set; }
    public AuctionRules Rules { get; private set; }
    public int TotalBids { get; private set; }
    public BidId? CurrentWinningBidId { get; private set; }
    
    // Factory Method
    public static Result<Auction> Create(
        string title,
        string description,
        Money startingPrice,
        Money reservePrice,
        Money bidIncrement,
        DateTime startDate,
        DateTime endDate,
        UserId sellerId,
        Category category,
        AuctionRules? rules = null)
    {
        // Validações
        if (string.IsNullOrWhiteSpace(title))
            return Result<Auction>.Failure(
                new Error("Auction.InvalidTitle", "Título é obrigatório"));
        
        if (title.Length > 255)
            return Result<Auction>.Failure(
                new Error("Auction.TitleTooLong", "Título deve ter no máximo 255 caracteres"));
        
        if (endDate <= startDate)
            return Result<Auction>.Failure(
                new Error("Auction.InvalidDates", "Data fim deve ser posterior à data início"));
        
        if (startDate <= DateTime.UtcNow)
            return Result<Auction>.Failure(
                new Error("Auction.InvalidStartDate", "Data início deve ser futura"));
        
        if (!reservePrice.IsGreaterThanOrEqual(startingPrice))
            return Result<Auction>.Failure(
                new Error("Auction.InvalidReserve", "Preço reserva deve ser >= preço inicial"));
        
        var auction = new Auction(
            title, description, startingPrice, reservePrice, bidIncrement,
            startDate, endDate, sellerId, category, rules ?? AuctionRules.CreateDefault());
        
        auction.RaiseDomainEvent(new AuctionCreatedEvent(auction.Id));
        
        return Result<Auction>.Success(auction);
    }
    
    // Business Methods
    public Result Schedule()
    {
        if (Status != AuctionStatus.Draft)
            return Result.Failure(
                new Error("Auction.InvalidStatus", "Leilão deve estar em rascunho"));
        
        Status = AuctionStatus.Scheduled;
        return Result.Success();
    }
    
    public Result Start()
    {
        if (Status != AuctionStatus.Scheduled)
            return Result.Failure(
                new Error("Auction.InvalidStatus", "Leilão deve estar agendado"));
        
        if (DateTime.UtcNow < StartDate)
            return Result.Failure(
                new Error("Auction.NotYetStarted", "Leilão ainda não começou"));
        
        Status = AuctionStatus.Active;
        RaiseDomainEvent(new AuctionStartedEvent(Id));
        
        return Result.Success();
    }
    
    public Result RegisterBid(BidId bidId, Money amount, UserId bidderId)
    {
        if (Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));
        
        // Atualizar estado do leilão
        CurrentPrice = amount;
        CurrentWinningBidId = bidId;
        WinnerId = bidderId;
        TotalBids++;
        
        // Extensão automática se lance nos últimos minutos
        var timeToEnd = EndDate - DateTime.UtcNow;
        if (timeToEnd <= Rules.ExtensionWindow && timeToEnd > TimeSpan.Zero)
        {
            EndDate = EndDate.Add(Rules.ExtensionTime);
            RaiseDomainEvent(new AuctionExtendedEvent(Id, EndDate));
        }
        
        return Result.Success();
    }
    
    public Result End()
    {
        if (Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));
        
        if (DateTime.UtcNow < EndDate)
            return Result.Failure(
                new Error("Auction.NotEnded", "Leilão ainda não terminou"));
        
        Status = AuctionStatus.Ended;
        
        // Verificar se atingiu o preço de reserva
        var hasWinner = CurrentPrice.IsGreaterThanOrEqual(ReservePrice);
        
        RaiseDomainEvent(new AuctionEndedEvent(Id, WinnerId, hasWinner));
        
        return Result.Success();
    }
    
    public Result Cancel(string reason)
    {
        if (Status == AuctionStatus.Ended || Status == AuctionStatus.Completed)
            return Result.Failure(
                new Error("Auction.CannotCancel", "Não é possível cancelar leilão finalizado"));
        
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(
                new Error("Auction.InvalidReason", "Motivo do cancelamento é obrigatório"));
        
        Status = AuctionStatus.Cancelled;
        RaiseDomainEvent(new AuctionCancelledEvent(Id, reason));
        
        return Result.Success();
    }
    
    public Result Pause(string reason)
    {
        if (Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));
        
        Status = AuctionStatus.Paused;
        return Result.Success();
    }
    
    public Result Resume()
    {
        if (Status != AuctionStatus.Paused)
            return Result.Failure(
                new Error("Auction.NotPaused", "Leilão não está pausado"));
        
        Status = AuctionStatus.Active;
        return Result.Success();
    }
}
```

---

#### **Bid Entity**

```csharp
// Auction.Domain/Entities/Bid.cs

public class Bid : BaseEntity
{
    protected Bid() { }
    
    private Bid(
        AuctionId auctionId,
        UserId bidderId,
        Money amount,
        ProxyBid? proxyBid = null) : base(Guid.NewGuid(), DateTime.UtcNow, null)
    {
        AuctionId = auctionId;
        BidderId = bidderId;
        Amount = amount;
        BidTime = DateTime.UtcNow;
        Status = BidStatus.Pending;
        ProxyBid = proxyBid;
        Version = 0;
        SequenceNumber = 0;
    }
    
    // Properties
    public AuctionId AuctionId { get; private set; }
    public UserId BidderId { get; private set; }
    public Money Amount { get; private set; }
    public DateTime BidTime { get; private set; }
    public BidStatus Status { get; private set; }
    public ProxyBid? ProxyBid { get; private set; }
    public long Version { get; private set; }
    public long SequenceNumber { get; private set; }
    
    // Factory Method
    public static Result<Bid> Create(
        AuctionId auctionId,
        UserId bidderId,
        Money amount,
        ProxyBid? proxyBid = null)
    {
        if (amount.Amount <= 0)
            return Result<Bid>.Failure(
                new Error("Bid.InvalidAmount", "Valor do lance deve ser maior que zero"));
        
        var bid = new Bid(auctionId, bidderId, amount, proxyBid);
        
        bid.RaiseDomainEvent(new BidPlacedEvent(
            bid.Id, auctionId.Value, bidderId.Value, amount.Amount, bid.BidTime));
        
        return Result<Bid>.Success(bid);
    }
    
    // Business Methods
    public Result Accept(long sequenceNumber)
    {
        if (Status != BidStatus.Pending)
            return Result.Failure(
                new Error("Bid.InvalidStatus", "Lance deve estar pendente"));
        
        Status = BidStatus.Accepted;
        SequenceNumber = sequenceNumber;
        Version++;
        
        RaiseDomainEvent(new BidAcceptedEvent(Id, AuctionId.Value, Amount.Amount));
        
        return Result.Success();
    }
    
    public Result Reject(string reason)
    {
        if (Status != BidStatus.Pending)
            return Result.Failure(
                new Error("Bid.InvalidStatus", "Lance deve estar pendente"));
        
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(
                new Error("Bid.InvalidReason", "Motivo da rejeição é obrigatório"));
        
        Status = BidStatus.Rejected;
        Version++;
        
        RaiseDomainEvent(new BidRejectedEvent(Id, AuctionId.Value, reason));
        
        return Result.Success();
    }
    
    public Result MarkAsWinning()
    {
        if (Status != BidStatus.Accepted)
            return Result.Failure(
                new Error("Bid.InvalidStatus", "Lance deve estar aceito"));
        
        Status = BidStatus.Winning;
        Version++;
        
        return Result.Success();
    }
    
    public Result MarkAsOutbid()
    {
        if (Status != BidStatus.Winning && Status != BidStatus.Accepted)
            return Result.Failure(
                new Error("Bid.InvalidStatus", "Lance não está vencendo"));
        
        Status = BidStatus.Outbid;
        Version++;
        
        RaiseDomainEvent(new BidOutbidEvent(Id, BidderId.Value));
        
        return Result.Success();
    }
    
    public Result MarkAsWon()
    {
        if (Status != BidStatus.Winning)
            return Result.Failure(
                new Error("Bid.InvalidStatus", "Lance deve estar vencendo"));
        
        Status = BidStatus.Won;
        Version++;
        
        return Result.Success();
    }
    
    public Result MarkAsLost()
    {
        if (Status != BidStatus.Outbid)
            return Result.Failure(
                new Error("Bid.InvalidStatus", "Lance deve estar superado"));
        
        Status = BidStatus.Lost;
        Version++;
        
        return Result.Success();
    }
}
```

---

## 📱 Application Layer

### Estrutura de Pastas

```
Application/
├── Commands/
│   ├── Auctions/
│   │   ├── CreateAuction/
│   │   │   ├── CreateAuctionCommand.cs
│   │   │   ├── CreateAuctionCommandHandler.cs
│   │   │   └── CreateAuctionCommandValidator.cs
│   │   ├── StartAuction/
│   │   │   ├── StartAuctionCommand.cs
│   │   │   └── StartAuctionCommandHandler.cs
│   │   ├── EndAuction/
│   │   ├── CancelAuction/
│   │   └── PauseAuction/
│   └── Bids/
│       ├── PlaceBid/
│       │   ├── PlaceBidCommand.cs
│       │   ├── PlaceBidCommandHandler.cs (salva Pending + publica Kafka)
│       │   └── PlaceBidCommandValidator.cs
│       └── ProcessBid/
│           └── ProcessBidCommandHandler.cs (Consumer Kafka - valida e aceita/rejeita)
├── Queries/ (CQRS - Read from Redis/PostgreSQL Read Replica)
│   ├── GetAuctionById/
│   │   ├── GetAuctionByIdQuery.cs
│   │   └── GetAuctionByIdQueryHandler.cs
│   ├── GetActiveAuctions/
│   │   ├── GetActiveAuctionsQuery.cs
│   │   └── GetActiveAuctionsQueryHandler.cs
│   ├── GetAuctionBids/
│   │   ├── GetAuctionBidsQuery.cs
│   │   └── GetAuctionBidsQueryHandler.cs
│   └── GetUserBids/
│       ├── GetUserBidsQuery.cs
│       └── GetUserBidsQueryHandler.cs
├── DTOs/
│   ├── AuctionDto.cs
│   ├── AuctionDetailsDto.cs
│   ├── BidDto.cs
│   └── UserBidHistoryDto.cs
├── Services/
│   ├── AuctionService.cs
│   ├── BidValidationService.cs
│   ├── ProxyBidService.cs
│   └── NotificationService.cs
└── Interfaces/
    ├── Repositories/
    │   ├── IAuctionRepository.cs
    │   ├── IBidRepository.cs
    │   └── IUserRepository.cs
    ├── Infrastructure/
    │   ├── IUnitOfWork.cs
    │   ├── IMessageBus.cs (Kafka)
    │   ├── ICacheService.cs (Redis)
    │   └── INotificationService.cs (SignalR)
    └── Services/
        ├── IAuctionService.cs
        └── IBidValidationService.cs
```

---

### Command Handlers - Implementação

#### **PlaceBidCommandHandler**

```csharp
// Application/Commands/Bids/PlaceBid/PlaceBidCommand.cs
public record PlaceBidCommand(
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    decimal? MaxProxyAmount = null) : IRequest<Result<Guid>>;

// Application/Commands/Bids/PlaceBid/PlaceBidCommandHandler.cs
public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, Result<Guid>>
{
    private readonly IBidRepository _bidRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IMessageBus _messageBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlaceBidCommandHandler> _logger;
    
    public PlaceBidCommandHandler(
        IBidRepository bidRepository,
        IAuctionRepository auctionRepository,
        IMessageBus messageBus,
        IUnitOfWork unitOfWork,
        ILogger<PlaceBidCommandHandler> logger)
    {
        _bidRepository = bidRepository;
        _auctionRepository = auctionRepository;
        _messageBus = messageBus;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Result<Guid>> Handle(PlaceBidCommand request, CancellationToken ct)
    {
        // 1. Validação básica rápida (não bloqueia processamento)
        var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, ct);
        if (auction is null)
            return Result<Guid>.Failure(
                new Error("Auction.NotFound", "Leilão não encontrado"));
        
        if (auction.Status != AuctionStatus.Active)
            return Result<Guid>.Failure(
                new Error("Auction.NotActive", "Leilão não está ativo"));
        
        if (auction.SellerId.Value == request.BidderId)
            return Result<Guid>.Failure(
                new Error("Bid.SellerCannotBid", "Vendedor não pode dar lance"));
        
        // 2. Criar lance (estado Pending)
        var moneyResult = Money.Create(request.Amount);
        if (!moneyResult.IsSuccess)
            return Result<Guid>.Failure(moneyResult.Error);
        
        ProxyBid? proxyBid = null;
        if (request.MaxProxyAmount.HasValue && request.MaxProxyAmount > request.Amount)
        {
            var maxMoneyResult = Money.Create(request.MaxProxyAmount.Value);
            if (maxMoneyResult.IsSuccess)
            {
                var proxyBidResult = ProxyBid.Create(maxMoneyResult.Value, moneyResult.Value);
                if (proxyBidResult.IsSuccess)
                    proxyBid = proxyBidResult.Value;
            }
        }
        
        var bidResult = Bid.Create(
            AuctionId.From(request.AuctionId),
            UserId.From(request.BidderId),
            moneyResult.Value,
            proxyBid);
        
        if (!bidResult.IsSuccess)
            return Result<Guid>.Failure(bidResult.Error);
        
        var bid = bidResult.Value;
        
        // 3. Salvar lance pendente no PostgreSQL
        await _bidRepository.AddAsync(bid, ct);
        await _unitOfWork.CommitAsync(ct);
        
        // 4. Publicar evento no Kafka para validação assíncrona
        // Partition Key = AuctionId (garante ordem sequencial)
        await _messageBus.PublishAsync(
            "bid-placed-topic",
            new BidPlacedEvent(
                bid.Id, 
                request.AuctionId, 
                request.BidderId, 
                request.Amount, 
                DateTime.UtcNow),
            partitionKey: request.AuctionId.ToString(),
            ct);
        
        _logger.LogInformation(
            "Bid {BidId} placed for auction {AuctionId} by user {BidderId}", 
            bid.Id, request.AuctionId, request.BidderId);
        
        return Result<Guid>.Success(bid.Id);
    }
}

// Application/Commands/Bids/PlaceBid/PlaceBidCommandValidator.cs
public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("AuctionId é obrigatório");
        
        RuleFor(x => x.BidderId)
            .NotEmpty()
            .WithMessage("BidderId é obrigatório");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero");
        
        RuleFor(x => x.MaxProxyAmount)
            .GreaterThan(x => x.Amount)
            .When(x => x.MaxProxyAmount.HasValue)
            .WithMessage("Valor máximo do proxy bid deve ser maior que o lance");
    }
}
```

---

#### **ProcessBidCommandHandler (Kafka Consumer)**

```csharp
// Application/Commands/Bids/ProcessBid/ProcessBidCommandHandler.cs
public class ProcessBidCommandHandler
{
    private readonly IBidRepository _bidRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly ICacheService _cache;
    private readonly INotificationService _notificationService;
    private readonly IMessageBus _messageBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessBidCommandHandler> _logger;
    
    public async Task ProcessAsync(BidPlacedEvent @event, CancellationToken ct)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessBid");
        activity?.SetTag("bid.id", @event.BidId);
        activity?.SetTag("auction.id", @event.AuctionId);
        
        try
        {
            // 1. Obter lance e leilão
            var bid = await _bidRepository.GetByIdAsync(@event.BidId, ct);
            var auction = await _auctionRepository.GetByIdAsync(@event.AuctionId, ct);
            
            if (bid is null || auction is null)
            {
                _logger.LogError("Bid {BidId} or Auction {AuctionId} not found", 
                    @event.BidId, @event.AuctionId);
                return;
            }
            
            // 2. Gerar sequence number atômico usando Redis INCR
            var sequenceKey = $"auction:{auction.Id}:sequence";
            var sequenceNumber = await _cache.IncrementAsync(sequenceKey, ct);
            
            // 3. Obter último lance aceito do cache
            var lastBidKey = $"auction:{auction.Id}:lastbid";
            var lastBidAmount = await _cache.GetAsync<decimal?>(lastBidKey, ct) 
                ?? auction.CurrentPrice.Amount;
            
            // 4. Validar lance
            var validationResult = ValidateBid(bid, auction, lastBidAmount);
            
            if (validationResult.IsSuccess)
            {
                // 5. ACEITAR LANCE
                bid.Accept(sequenceNumber);
                bid.MarkAsWinning();
                
                // 6. Atualizar leilão
                auction.RegisterBid(
                    BidId.From(bid.Id), 
                    bid.Amount, 
                    bid.BidderId);
                
                // 7. Marcar lance anterior como Outbid
                if (auction.CurrentWinningBidId is not null)
                {
                    var previousBid = await _bidRepository.GetByIdAsync(
                        auction.CurrentWinningBidId.Value, ct);
                    
                    if (previousBid is not null)
                    {
                        previousBid.MarkAsOutbid();
                        
                        // Notificar usuário que foi superado
                        await _notificationService.NotifyBidOutbidAsync(
                            previousBid.BidderId.Value,
                            previousBid.Id,
                            bid.Amount.Amount,
                            ct);
                    }
                }
                
                // 8. Atualizar cache (último lance aceito)
                await _cache.SetAsync(
                    lastBidKey, 
                    bid.Amount.Amount, 
                    TimeSpan.FromMinutes(30), 
                    ct);
                
                // 9. Invalidar cache de leilões ativos
                await _cache.RemoveAsync("active-auctions", ct);
                await _cache.RemoveAsync($"auction:{auction.Id}", ct);
                
                // 10. Salvar mudanças
                await _unitOfWork.CommitAsync(ct);
                
                // 11. Notificar todos os usuários via SignalR
                await _notificationService.NotifyBidAcceptedAsync(
                    auction.Id,
                    bid.BidderId.Value,
                    bid.Amount.Amount,
                    sequenceNumber,
                    ct);
                
                // 12. Processar Proxy Bids (lances automáticos)
                await ProcessProxyBidsAsync(auction.Id, bid.Amount, bid.BidderId, ct);
                
                _logger.LogInformation(
                    "Bid {BidId} accepted for auction {AuctionId} with sequence {Sequence}",
                    bid.Id, auction.Id, sequenceNumber);
            }
            else
            {
                // 13. REJEITAR LANCE
                bid.Reject(validationResult.Error.Message);
                await _unitOfWork.CommitAsync(ct);
                
                // 14. Notificar usuário
                await _notificationService.NotifyBidRejectedAsync(
                    bid.BidderId.Value,
                    bid.Id,
                    validationResult.Error.Message,
                    ct);
                
                _logger.LogWarning(
                    "Bid {BidId} rejected for auction {AuctionId}: {Reason}",
                    bid.Id, auction.Id, validationResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bid {BidId}", @event.BidId);
            throw;
        }
    }
    
    private Result ValidateBid(Bid bid, Auction auction, decimal lastBidAmount)
    {
        // Validação 1: Leilão ainda está ativo?
        if (auction.Status != AuctionStatus.Active)
            return Result.Failure(
                new Error("Bid.AuctionNotActive", "Leilão não está mais ativo"));
        
        // Validação 2: Lance é maior que o atual?
        if (bid.Amount.Amount <= lastBidAmount)
            return Result.Failure(
                new Error("Bid.TooLow", "Lance deve ser maior que o atual"));
        
        // Validação 3: Respeita incremento mínimo?
        var minimumBid = lastBidAmount + auction.BidIncrement.Amount;
        if (bid.Amount.Amount < minimumBid)
            return Result.Failure(
                new Error("Bid.IncrementTooLow", 
                    $"Lance deve ser no mínimo {minimumBid:C}"));
        
        // Validação 4: Vendedor não pode dar lance
        if (bid.BidderId == auction.SellerId)
            return Result.Failure(
                new Error("Bid.SellerCannotBid", "Vendedor não pode dar lance"));
        
        // Validação 5: Leilão não está expirado?
        if (DateTime.UtcNow > auction.EndDate)
            return Result.Failure(
                new Error("Bid.AuctionExpired", "Leilão já expirou"));
        
        return Result.Success();
    }
    
    private async Task ProcessProxyBidsAsync(
        Guid auctionId, 
        Money currentBid, 
        UserId currentBidderId,
        CancellationToken ct)
    {
        // Buscar proxy bids ativos para este leilão (exceto do usuário que deu o lance)
        var proxyBids = await _bidRepository.GetActiveProxyBidsAsync(
            auctionId, 
            excludeBidderId: currentBidderId.Value,
            ct);
        
        foreach (var proxyBid in proxyBids.OrderByDescending(b => b.ProxyBid!.MaxAmount.Amount))
        {
            if (proxyBid.ProxyBid!.MaxAmount.IsGreaterThan(currentBid))
            {
                // Calcular novo valor (lance atual + incremento)
                var auction = await _auctionRepository.GetByIdAsync(auctionId, ct);
                if (auction is null) continue;
                
                var newAmount = currentBid.Add(auction.BidIncrement);
                
                // Não ultrapassar o máximo do proxy bid
                if (newAmount.IsGreaterThan(proxyBid.ProxyBid.MaxAmount))
                    newAmount = proxyBid.ProxyBid.MaxAmount;
                
                // Publicar novo lance automático
                await _messageBus.PublishAsync(
                    "bid-placed-topic",
                    new BidPlacedEvent(
                        Guid.NewGuid(),
                        auctionId,
                        proxyBid.BidderId.Value,
                        newAmount.Amount,
                        DateTime.UtcNow),
                    partitionKey: auctionId.ToString(),
                    ct);
                
                _logger.LogInformation(
                    "Proxy bid triggered for user {UserId} on auction {AuctionId}",
                    proxyBid.BidderId.Value, auctionId);
                
                // Processar apenas o primeiro proxy bid válido
                break;
            }
        }
    }
}
```

---

### Query Handlers (CQRS Read Side)

```csharp
// Application/Queries/GetActiveAuctions/GetActiveAuctionsQuery.cs
public record GetActiveAuctionsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? CategoryId = null) : IRequest<Result<PagedResult<AuctionDto>>>;

// Application/Queries/GetActiveAuctions/GetActiveAuctionsQueryHandler.cs
public class GetActiveAuctionsQueryHandler 
    : IRequestHandler<GetActiveAuctionsQuery, Result<PagedResult<AuctionDto>>>
{
    private readonly ICacheService _cache;
    private readonly IAuctionRepository _repository;
    
    public async Task<Result<PagedResult<AuctionDto>>> Handle(
        GetActiveAuctionsQuery request, 
        CancellationToken ct)
    {
        var cacheKey = $"active-auctions:page:{request.PageNumber}:size:{request.PageSize}:cat:{request.CategoryId}";
        
        // 1. Tentar cache Redis
        var cached = await _cache.GetAsync<PagedResult<AuctionDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<PagedResult<AuctionDto>>.Success(cached);
        
        // 2. Buscar do DB (Read Replica)
        var auctions = await _repository.GetActiveAuctionsAsync(
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            ct);
        
        var totalCount = await _repository.GetActiveAuctionsCountAsync(
            request.CategoryId, ct);
        
        var dtos = auctions.Select(MapToDto).ToList();
        
        var result = new PagedResult<AuctionDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);
        
        // 3. Cachear por 30 segundos (atualiza frequentemente)
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(30), ct);
        
        return Result<PagedResult<AuctionDto>>.Success(result);
    }
    
    private AuctionDto MapToDto(Auction auction) => new(
        auction.Id,
        auction.Title,
        auction.Description,
        auction.CurrentPrice.Amount,
        auction.StartingPrice.Amount,
        auction.ReservePrice.Amount,
        auction.BidIncrement.Amount,
        auction.EndDate,
        auction.Status.ToString(),
        auction.TotalBids);
}
```

---

## 🔄 Estratégia de Concorrência - DETALHADA

### Fluxo Completo de um Lance

```
┌────────────┐
│   Client   │ (React/Vue/Mobile)
└──────┬─────┘
       │ HTTP POST /api/bids
       ▼
┌─────────────────────────────────────────────────────────┐
│                    API Gateway                           │
│  - Rate Limiting (100 req/seg por usuário)             │
│  - Authentication (JWT)                                  │
│  - Basic Validation                                      │
└──────┬──────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────┐
│            PlaceBidCommandHandler                        │
│  1. Validação rápida (leilão ativo, valor > 0)         │
│  2. Criar Bid (Status = Pending)                        │
│  3. Salvar PostgreSQL                                    │
│  4. Publicar Kafka ("bid-placed-topic")                │
│  5. Retornar HTTP 202 Accepted (BidId)                 │
└──────┬──────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────┐
│                   Apache Kafka                           │
│  Topic: bid-placed-topic                                 │
│  Partitions: 10 (partitioned by AuctionId)              │
│  Retention: 7 days                                       │
└──────┬──────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────┐
│          BidValidationConsumer (Worker)                  │
│  - Consume sequencialmente por partition                │
│  - 1 consumer por partition (paralelismo)               │
└──────┬──────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────┐
│         ProcessBidCommandHandler                         │
│  1. Redis INCR - Sequence Number atômico               │
│  2. Redis GET - Último lance aceito                     │
│  3. Validar lance (valor, incremento, expiração)        │
│  4. SE VÁLIDO:                                           │
│     - Aceitar lance (Status = Accepted)                 │
│     - Atualizar leilão (CurrentPrice, WinnerId)         │
│     - Marcar lance anterior como Outbid                 │
│     - Redis SET - Novo último lance                     │
│     - Invalidar cache                                    │
│     - Salvar PostgreSQL                                  │
│     - SignalR - Notificar todos                         │
│     - Processar Proxy Bids                              │
│  5. SE INVÁLIDO:                                         │
│     - Rejeitar lance (Status = Rejected)                │
│     - SignalR - Notificar usuário                       │
└──────┬──────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────┐
│                   SignalR Hub                            │
│  - Broadcast para grupo "auction-{id}"                  │
│  - Push notification para cliente                        │
└──────┬──────────────────────────────────────────────────┘
       │
       ▼
┌────────────┐
│   Client   │ (Atualização em tempo real)
└────────────┘
```

---

### Kafka Partitioning Strategy

**Por que particionar por AuctionId?**

```csharp
// Infrastructure/Messaging/KafkaProducer.cs
public class KafkaProducer : IMessageBus
{
    private readonly IProducer<string, string> _producer;
    
    public async Task PublishAsync<TEvent>(
        string topic, 
        TEvent @event, 
        string partitionKey,
        CancellationToken ct) where TEvent : IDomainEvent
    {
        var message = JsonSerializer.Serialize(@event);
        
        // Partition Key = AuctionId
        // Todos os lances do MESMO leilão vão para a MESMA partition
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = partitionKey, // AuctionId
            Value = message,
            Timestamp = Timestamp.Default
        }, ct);
    }
}
```

**Garantias:**
- ✅ Lances do **mesmo leilão** sempre na **mesma partition**
- ✅ Consumer processa **sequencialmente** por partition
- ✅ **Ordem garantida** dentro do leilão
- ✅ **Paralelismo** entre leilões diferentes

**Exemplo:**
```
Auction A → Partition 1 → Consumer 1 (processa sequencialmente)
Auction B → Partition 2 → Consumer 2 (processa sequencialmente)
Auction C → Partition 1 → Consumer 1 (processa sequencialmente)
```

---

### Redis como Coordenador de Concorrência

#### **Sequence Number Atômico**

```csharp
// Infrastructure/Caching/RedisCacheService.cs
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    
    // INCR é uma operação atômica no Redis
    public async Task<long> IncrementAsync(string key, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        return await db.StringIncrementAsync(key);
    }
}
```

**Por que Redis INCR?**
- ⚡ **Atômico**: Não há race condition
- 🚀 **Performance**: Sub-milissegundo
- 📊 **Sequencial**: Garante ordem única
- 🔒 **Lock-Free**: Não bloqueia threads

**Uso:**
```csharp
// Cada lance recebe um número sequencial único
var sequenceNumber = await _cache.IncrementAsync($"auction:{auctionId}:sequence", ct);
// Retorna: 1, 2, 3, 4, 5... (sempre crescente e único)
```

---

#### **Cache do Último Lance**

```csharp
public async Task<decimal?> GetLastBidAmountAsync(Guid auctionId, CancellationToken ct)
{
    var key = $"auction:{auctionId}:lastbid";
    return await _cache.GetAsync<decimal?>(key, ct);
}

public async Task SetLastBidAmountAsync(
    Guid auctionId, 
    decimal amount, 
    CancellationToken ct)
{
    var key = $"auction:{auctionId}:lastbid";
    await _cache.SetAsync(key, amount, TimeSpan.FromMinutes(30), ct);
}
```

**Vantagens:**
- 🚀 Leitura ultra-rápida (sem query no DB)
- ✅ Validação imediata
- 🔄 Auto-expira após 30 minutos (fallback para DB)

---

### Optimistic Locking no PostgreSQL

```csharp
// Domain/Entities/Base/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public long Version { get; protected set; } // Concurrency Token
}

// Infrastructure/Persistence/AppDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Auction>()
        .Property(a => a.Version)
        .IsConcurrencyToken();
    
    modelBuilder.Entity<Bid>()
        .Property(b => b.Version)
        .IsConcurrencyToken();
}

// Infrastructure/Persistence/UnitOfWork.cs
public override async Task<int> SaveChangesAsync(CancellationToken ct)
{
    try
    {
        // Auto-increment version
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        return await base.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // Conflito de versão detectado
        throw new ConcurrencyException(
            "Registro foi modificado por outro processo", ex);
    }
}
```

---

### Distributed Lock (Para Casos Extremos)

```csharp
// Infrastructure/Caching/RedisCacheService.cs
public async Task<bool> AcquireLockAsync(
    string key, 
    TimeSpan expiry, 
    CancellationToken ct)
{
    var db = _redis.GetDatabase();
    var lockValue = Guid.NewGuid().ToString();
    
    // SET key value NX PX milliseconds
    return await db.StringSetAsync(
        key, 
        lockValue, 
        expiry, 
        When.NotExists);
}

public async Task ReleaseLockAsync(string key, CancellationToken ct)
{
    var db = _redis.GetDatabase();
    await db.KeyDeleteAsync(key);
}

// Uso (se necessário)
var lockKey = $"auction:{auctionId}:lock";
var acquired = await _cache.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(5), ct);

if (acquired)
{
    try
    {
        // Operação crítica
    }
    finally
    {
        await _cache.ReleaseLockAsync(lockKey, ct);
    }
}
```

---

## 💾 Infrastructure Layer

### PostgreSQL Database Schema

```sql
-- ==================================================
-- USERS TABLE (Table Per Hierarchy)
-- ==================================================
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    user_type VARCHAR(50) NOT NULL, -- 'Individual' or 'Corporate'
    
    -- Individual User fields
    cpf VARCHAR(11) NULL,
    
    -- Corporate User fields
    cnpj VARCHAR(14) NULL,
    company_name VARCHAR(255) NULL,
    
    -- Audit fields
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    version BIGINT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT chk_user_type CHECK (user_type IN ('Individual', 'Corporate')),
    CONSTRAINT chk_individual_cpf CHECK (
        (user_type = 'Individual' AND cpf IS NOT NULL) OR 
        (user_type = 'Corporate')
    ),
    CONSTRAINT chk_corporate_cnpj CHECK (
        (user_type = 'Corporate' AND cnpj IS NOT NULL AND company_name IS NOT NULL) OR 
        (user_type = 'Individual')
    )
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_cpf ON users(cpf) WHERE cpf IS NOT NULL;
CREATE INDEX idx_users_cnpj ON users(cnpj) WHERE cnpj IS NOT NULL;

-- ==================================================
-- CATEGORIES TABLE
-- ==================================================
CREATE TABLE categories (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    version BIGINT NOT NULL DEFAULT 0
);

CREATE INDEX idx_categories_name ON categories(name);

-- ==================================================
-- AUCTIONS TABLE
-- ==================================================
CREATE TABLE auctions (
    id UUID PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    
    -- Pricing
    starting_price_amount DECIMAL(18,2) NOT NULL,
    starting_price_currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    reserve_price_amount DECIMAL(18,2) NOT NULL,
    reserve_price_currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    buy_now_price_amount DECIMAL(18,2) NULL,
    buy_now_price_currency VARCHAR(3) NULL,
    current_price_amount DECIMAL(18,2) NOT NULL,
    current_price_currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    bid_increment_amount DECIMAL(18,2) NOT NULL,
    bid_increment_currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    
    -- Dates
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    
    -- Status
    status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    
    -- Relationships
    seller_id UUID NOT NULL REFERENCES users(id),
    winner_id UUID NULL REFERENCES users(id),
    category_id UUID NOT NULL REFERENCES categories(id),
    current_winning_bid_id UUID NULL,
    
    -- Stats
    total_bids INT NOT NULL DEFAULT 0,
    
    -- Auction Rules (JSON for flexibility)
    rules JSONB NOT NULL DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    version BIGINT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT chk_auction_status CHECK (
        status IN ('Draft', 'Scheduled', 'Active', 'Paused', 'Ended', 
                   'Cancelled', 'AwaitingPayment', 'Completed')
    ),
    CONSTRAINT chk_auction_dates CHECK (end_date > start_date),
    CONSTRAINT chk_auction_prices CHECK (reserve_price_amount >= starting_price_amount)
);

-- Indexes for performance
CREATE INDEX idx_auctions_status ON auctions(status);
CREATE INDEX idx_auctions_end_date ON auctions(end_date);
CREATE INDEX idx_auctions_seller ON auctions(seller_id);
CREATE INDEX idx_auctions_category ON auctions(category_id);
CREATE INDEX idx_auctions_active_ending ON auctions(end_date, status) 
    WHERE status = 'Active';

-- ==================================================
-- BIDS TABLE (High Volume - Consider Partitioning)
-- ==================================================
CREATE TABLE bids (
    id UUID PRIMARY KEY,
    auction_id UUID NOT NULL REFERENCES auctions(id),
    bidder_id UUID NOT NULL REFERENCES users(id),
    
    -- Bid details
    amount_value DECIMAL(18,2) NOT NULL,
    amount_currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    bid_time TIMESTAMP NOT NULL DEFAULT NOW(),
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    sequence_number BIGINT NOT NULL DEFAULT 0,
    
    -- Proxy Bid (JSON)
    proxy_bid JSONB NULL,
    
    -- Audit fields
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    version BIGINT NOT NULL DEFAULT 0,
    
    -- Constraints
    CONSTRAINT chk_bid_status CHECK (
        status IN ('Pending', 'Accepted', 'Rejected', 'Outbid', 
                   'Winning', 'Won', 'Lost')
    ),
    CONSTRAINT chk_bid_amount CHECK (amount_value > 0)
);

-- Indexes for high-performance queries
CREATE INDEX idx_bids_auction ON bids(auction_id);
CREATE INDEX idx_bids_bidder ON bids(bidder_id);
CREATE INDEX idx_bids_sequence ON bids(auction_id, sequence_number DESC);
CREATE INDEX idx_bids_status ON bids(status);
CREATE INDEX idx_bids_bid_time ON bids(bid_time DESC);
CREATE INDEX idx_bids_winning ON bids(auction_id, status) 
    WHERE status = 'Winning';

-- ==================================================
-- TABLE PARTITIONING FOR BIDS (OPTIONAL)
-- Para escala muito grande (milhões de lances)
-- ==================================================
-- Partition by month
CREATE TABLE bids_partitioned (
    LIKE bids INCLUDING ALL
) PARTITION BY RANGE (bid_time);

CREATE TABLE bids_2025_01 PARTITION OF bids_partitioned
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

CREATE TABLE bids_2025_02 PARTITION OF bids_partitioned
    FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');

-- Continue criando partições conforme necessário...
```

---

### Repository Implementation

```csharp
// Infrastructure/Persistence/Repositories/AuctionRepository.cs
public class AuctionRepository : IAuctionRepository
{
    private readonly AppDbContext _context;
    
    public AuctionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Auctions
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }
    
    public async Task<List<Auction>> GetActiveAuctionsAsync(
        int pageNumber,
        int pageSize,
        string? categoryId,
        CancellationToken ct)
    {
        var query = _context.Auctions
            .Include(a => a.Category)
            .Where(a => a.Status == AuctionStatus.Active);
        
        if (!string.IsNullOrEmpty(categoryId))
            query = query.Where(a => a.Category.Id == Guid.Parse(categoryId));
        
        return await query
            .OrderBy(a => a.EndDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
    
    public async Task<int> GetActiveAuctionsCountAsync(
        string? categoryId,
        CancellationToken ct)
    {
        var query = _context.Auctions
            .Where(a => a.Status == AuctionStatus.Active);
        
        if (!string.IsNullOrEmpty(categoryId))
            query = query.Where(a => a.Category.Id == Guid.Parse(categoryId));
        
        return await query.CountAsync(ct);
    }
    
    public async Task<List<Auction>> GetScheduledAuctionsReadyToStartAsync(
        CancellationToken ct)
    {
        return await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Scheduled 
                && a.StartDate <= DateTime.UtcNow)
            .ToListAsync(ct);
    }
    
    public async Task<List<Auction>> GetExpiredActiveAuctionsAsync(
        CancellationToken ct)
    {
        return await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Active 
                && a.EndDate <= DateTime.UtcNow)
            .ToListAsync(ct);
    }
    
    public async Task AddAsync(Auction auction, CancellationToken ct)
    {
        await _context.Auctions.AddAsync(auction, ct);
    }
    
    public void Update(Auction auction)
    {
        _context.Auctions.Update(auction);
    }
}

// Infrastructure/Persistence/Repositories/BidRepository.cs
public class BidRepository : IBidRepository
{
    private readonly AppDbContext _context;
    
    public BidRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Bid?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Bids
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }
    
    public async Task<List<Bid>> GetByAuctionIdAsync(
        Guid auctionId,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        return await _context.Bids
            .Where(b => b.AuctionId == AuctionId.From(auctionId))
            .OrderByDescending(b => b.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
    
    public async Task<List<Bid>> GetActiveProxyBidsAsync(
        Guid auctionId,
        Guid excludeBidderId,
        CancellationToken ct)
    {
        return await _context.Bids
            .Where(b => b.AuctionId == AuctionId.From(auctionId)
                && b.BidderId != UserId.From(excludeBidderId)
                && b.ProxyBid != null
                && b.Status == BidStatus.Accepted)
            .OrderByDescending(b => b.ProxyBid!.MaxAmount.Amount)
            .ToListAsync(ct);
    }
    
    public async Task AddAsync(Bid bid, CancellationToken ct)
    {
        await _context.Bids.AddAsync(bid, ct);
    }
    
    public void Update(Bid bid)
    {
        _context.Bids.Update(bid);
    }
}
```

---

### Redis Cache Service

```csharp
// Infrastructure/Caching/RedisCacheService.cs
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    
    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
                return default;
            
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key {Key}", key);
            return default;
        }
    }
    
    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan expiry, 
        CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serialized, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key}", key);
        }
    }
    
    public async Task<long> IncrementAsync(string key, CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.StringIncrementAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cache key {Key}", key);
            throw;
        }
    }
    
    public async Task<bool> AcquireLockAsync(
        string key, 
        TimeSpan expiry, 
        CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            var lockValue = Guid.NewGuid().ToString();
            return await db.StringSetAsync(key, lockValue, expiry, When.NotExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock {Key}", key);
            return false;
        }
    }
    
    public async Task ReleaseLockAsync(string key, CancellationToken ct)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock {Key}", key);
        }
    }
}
```

---

### Kafka Message Bus

```csharp
// Infrastructure/Messaging/KafkaProducer.cs
public class KafkaProducer : IMessageBus
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    
    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            Acks = Acks.All, // Wait for all replicas
            EnableIdempotence = true, // Guarantee exactly-once
            MaxInFlight = 5,
            MessageSendMaxRetries = 3,
            CompressionType = CompressionType.Snappy
        };
        
        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger = logger;
    }
    
    public async Task PublishAsync<TEvent>(
        string topic,
        TEvent @event,
        string partitionKey,
        CancellationToken ct) where TEvent : IDomainEvent
    {
        try
        {
            var message = JsonSerializer.Serialize(@event);
            
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = partitionKey,
                Value = message,
                Timestamp = Timestamp.Default
            }, ct);
            
            _logger.LogInformation(
                "Event published to Kafka: Topic={Topic}, Partition={Partition}, Offset={Offset}",
                result.Topic, result.Partition.Value, result.Offset.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to Kafka: {@Event}", @event);
            throw;
        }
    }
    
    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

// Infrastructure/Messaging/KafkaConsumer.cs
public class BidValidationConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BidValidationConsumer> _logger;
    
    public BidValidationConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<BidValidationConsumer> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "bid-validation-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Manual commit after processing
            MaxPollIntervalMs = 300000 // 5 minutes
        };
        
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _consumer.Subscribe("bid-placed-topic");
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(ct);
                
                var @event = JsonSerializer.Deserialize<BidPlacedEvent>(
                    consumeResult.Message.Value);
                
                if (@event is not null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetRequiredService<ProcessBidCommandHandler>();
                    
                    await handler.ProcessAsync(@event, ct);
                    
                    // Commit offset only after successful processing
                    _consumer.Commit(consumeResult);
                    
                    _logger.LogInformation(
                        "Bid {BidId} processed from partition {Partition} offset {Offset}",
                        @event.BidId, 
                        consumeResult.Partition.Value, 
                        consumeResult.Offset.Value);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message from Kafka");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bid event");
                // Dead Letter Queue aqui se necessário
            }
        }
        
        _consumer.Close();
    }
}
```

---

## 🔔 SignalR - Notificações em Tempo Real

```csharp
// Infrastructure/SignalR/AuctionHub.cs
[Authorize]
public class AuctionHub : Hub
{
    private readonly ILogger<AuctionHub> _logger;
    
    public AuctionHub(ILogger<AuctionHub> logger)
    {
        _logger = logger;
    }
    
    public async Task JoinAuction(string auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        
        _logger.LogInformation(
            "User {UserId} joined auction {AuctionId}",
            Context.UserIdentifier, auctionId);
    }
    
    public async Task LeaveAuction(string auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        
        _logger.LogInformation(
            "User {UserId} left auction {AuctionId}",
            Context.UserIdentifier, auctionId);
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

// Application/Services/SignalRNotificationService.cs
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;
    
    public SignalRNotificationService(
        IHubContext<AuctionHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task NotifyBidAcceptedAsync(
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        long sequenceNumber,
        CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients
                .Group($"auction-{auctionId}")
                .SendAsync("BidAccepted", new
                {
                    AuctionId = auctionId,
                    BidderId = bidderId,
                    Amount = amount,
                    SequenceNumber = sequenceNumber,
                    Timestamp = DateTime.UtcNow
                }, ct);
            
            _logger.LogInformation(
                "Notified bid accepted for auction {AuctionId}",
                auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bid accepted notification");
        }
    }
    
    public async Task NotifyBidRejectedAsync(
        Guid bidderId,
        Guid bidId,
        string reason,
        CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients
                .User(bidderId.ToString())
                .SendAsync("BidRejected", new
                {
                    BidId = bidId,
                    Reason = reason,
                    Timestamp = DateTime.UtcNow
                }, ct);
            
            _logger.LogInformation(
                "Notified user {UserId} of rejected bid {BidId}",
                bidderId, bidId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bid rejected notification");
        }
    }
    
    public async Task NotifyBidOutbidAsync(
        Guid bidderId,
        Guid bidId,
        decimal newAmount,
        CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients
                .User(bidderId.ToString())
                .SendAsync("BidOutbid", new
                {
                    BidId = bidId,
                    NewAmount = newAmount,
                    Timestamp = DateTime.UtcNow
                }, ct);
            
            _logger.LogInformation(
                "Notified user {UserId} of outbid {BidId}",
                bidderId, bidId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bid outbid notification");
        }
    }
    
    public async Task NotifyAuctionEndingAsync(
        Guid auctionId,
        TimeSpan timeLeft,
        CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients
                .Group($"auction-{auctionId}")
                .SendAsync("AuctionEnding", new
                {
                    AuctionId = auctionId,
                    TimeLeftSeconds = (int)timeLeft.TotalSeconds,
                    Timestamp = DateTime.UtcNow
                }, ct);
            
            _logger.LogInformation(
                "Notified auction {AuctionId} ending soon",
                auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction ending notification");
        }
    }
}
```

---

## ⚙️ Background Jobs

```csharp
// Infrastructure/Jobs/AuctionSchedulerJob.cs
public class AuctionSchedulerJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionSchedulerJob> _logger;
    
    public AuctionSchedulerJob(
        IServiceProvider serviceProvider,
        ILogger<AuctionSchedulerJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Auction Scheduler Job started");
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var auctionService = scope.ServiceProvider
                    .GetRequiredService<IAuctionService>();
                
                // 1. Iniciar leilões agendados
                var started = await auctionService.StartScheduledAuctionsAsync(ct);
                if (started > 0)
                    _logger.LogInformation("{Count} auctions started", started);
                
                // 2. Finalizar leilões expirados
                var ended = await auctionService.EndExpiredAuctionsAsync(ct);
                if (ended > 0)
                    _logger.LogInformation("{Count} auctions ended", ended);
                
                // 3. Notificar leilões próximos do fim (últimos 5 min)
                var notified = await auctionService.NotifyEndingAuctionsAsync(ct);
                if (notified > 0)
                    _logger.LogInformation("{Count} ending auctions notified", notified);
                
                // Executar a cada 30 segundos
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Auction Scheduler Job");
                await Task.Delay(TimeSpan.FromMinutes(1), ct);
            }
        }
        
        _logger.LogInformation("Auction Scheduler Job stopped");
    }
}

// Application/Services/AuctionService.cs
public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionService> _logger;
    
    public async Task<int> StartScheduledAuctionsAsync(CancellationToken ct)
    {
        var auctions = await _repository.GetScheduledAuctionsReadyToStartAsync(ct);
        
        foreach (var auction in auctions)
        {
            var result = auction.Start();
            if (result.IsSuccess)
            {
                _logger.LogInformation("Auction {AuctionId} started", auction.Id);
            }
        }
        
        await _unitOfWork.CommitAsync(ct);
        return auctions.Count;
    }
    
    public async Task<int> EndExpiredAuctionsAsync(CancellationToken ct)
    {
        var auctions = await _repository.GetExpiredActiveAuctionsAsync(ct);
        
        foreach (var auction in auctions)
        {
            var result = auction.End();
            if (result.IsSuccess)
            {
                _logger.LogInformation("Auction {AuctionId} ended", auction.Id);
            }
        }
        
        await _unitOfWork.CommitAsync(ct);
        return auctions.Count;
    }
    
    public async Task<int> NotifyEndingAuctionsAsync(CancellationToken ct)
    {
        var fiveMinutesFromNow = DateTime.UtcNow.AddMinutes(5);
        var auctions = await _repository.GetAuctionsEndingBeforeAsync(
            fiveMinutesFromNow, ct);
        
        foreach (var auction in auctions)
        {
            var timeLeft = auction.EndDate - DateTime.UtcNow;
            await _notificationService.NotifyAuctionEndingAsync(
                auction.Id, timeLeft, ct);
        }
        
        return auctions.Count;
    }
}
```

---

## 📊 Monitoramento e Observabilidade

### Métricas (Prometheus)

```csharp
// Application/Monitoring/AuctionMetrics.cs
public class AuctionMetrics
{
    private static readonly Counter BidsPlaced = Metrics
        .CreateCounter("auction_bids_placed_total", "Total number of bids placed");
    
    private static readonly Counter BidsAccepted = Metrics
        .CreateCounter("auction_bids_accepted_total", "Total number of bids accepted");
    
    private static readonly Counter BidsRejected = Metrics
        .CreateCounter("auction_bids_rejected_total", "Total number of bids rejected");
    
    private static readonly Histogram BidProcessingDuration = Metrics
        .CreateHistogram("auction_bid_processing_duration_seconds",
            "Duration of bid processing in seconds");
    
    private static readonly Gauge ActiveAuctions = Metrics
        .CreateGauge("auction_active_auctions_count", "Number of active auctions");
    
    private static readonly Gauge ActiveConnections = Metrics
        .CreateGauge("auction_signalr_connections_count", "Number of active SignalR connections");
    
    public void RecordBidPlaced() => BidsPlaced.Inc();
    public void RecordBidAccepted() => BidsAccepted.Inc();
    public void RecordBidRejected() => BidsRejected.Inc();
    
    public void RecordBidProcessingTime(TimeSpan duration) 
        => BidProcessingDuration.Observe(duration.TotalSeconds);
    
    public void UpdateActiveAuctions(int count) 
        => ActiveAuctions.Set(count);
    
    public void UpdateActiveConnections(int count) 
        => ActiveConnections.Set(count);
}
```

---

### Logging (Serilog)

```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"]!)
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"]!,
            TelemetryConverter.Traces);
});
```

---

### Tracing (OpenTelemetry)

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRedisInstrumentation()
        .AddSource("Auction.*")
        .AddJaegerExporter(options =>
        {
            options.AgentHost = builder.Configuration["Jaeger:AgentHost"];
            options.AgentPort = int.Parse(builder.Configuration["Jaeger:AgentPort"]!);
        }));
```

---

## 🚀 Deployment & Scaling

### Docker Compose (Local Development)

```yaml
version: '3.8'

services:
  # PostgreSQL
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: auction
      POSTGRES_USER: auction_user
      POSTGRES_PASSWORD: auction_pass
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  # Redis
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  # Kafka + Zookeeper
  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1

  # Auction API
  auction-api:
    build:
      context: .
      dockerfile: Auction.Api/Dockerfile
    ports:
      - "5000:8080"
    depends_on:
      - postgres
      - redis
      - kafka
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=auction;Username=auction_user;Password=auction_pass"
      Redis__ConnectionString: "redis:6379"
      Kafka__BootstrapServers: "kafka:9092"

volumes:
  postgres_data:
  redis_data:
```

---

### Kubernetes (Production)

```yaml
# auction-api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auction-api
spec:
  replicas: 5
  selector:
    matchLabels:
      app: auction-api
  template:
    metadata:
      labels:
        app: auction-api
    spec:
      containers:
      - name: auction-api
        image: auction-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: auction-secrets
              key: postgres-connection
        - name: Redis__ConnectionString
          value: "redis-cluster:6379"
        - name: Kafka__BootstrapServers
          value: "kafka-cluster:9092"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: auction-api-service
spec:
  selector:
    app: auction-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

---

## 📈 Performance Benchmarks

### Cenários de Teste

```
Scenario 1: Alta Concorrência
├── 3.000 usuários simultâneos
├── 1 leilão ativo
├── Taxa de 50 lances/segundo
├── Duração: 10 minutos
└── Resultado Esperado: < 500ms p95, 0% erros

Scenario 2: Múltiplos Leilões
├── 100 leilões ativos
├── 1.000 usuários distribuídos
├── Taxa de 200 lances/segundo (total)
├── Duração: 30 minutos
└── Resultado Esperado: < 1s p95, < 0.1% erros

Scenario 3: Pico de Fim de Leilão
├── 1 leilão expirando
├── 5.000 usuários tentando lance final
├── Taxa de 500 lances/segundo (spike)
├── Duração: 2 minutos
└── Resultado Esperado: < 2s p95, ordem correta garantida
```

---

## 📚 Referências e Próximos Passos

### Implementado
✅ Domain Layer (Entities, Value Objects, Events)  
✅ Application Layer (Commands, Queries, Handlers)  
✅ Infrastructure Layer (Repositories, Cache, Messaging)  
✅ Concurrency Strategy (Kafka + Redis + Optimistic Locking)  
✅ Real-time Notifications (SignalR)  
✅ Background Jobs (Scheduler)  

### Próximos Passos
⏳ Payment Integration (Gateway de Pagamento)  
⏳ Image Upload & CDN (Fotos dos produtos)  
⏳ Search & Filters (Elasticsearch)  
⏳ Admin Dashboard (Gestão de leilões)  
⏳ Mobile App (React Native / Flutter)  
⏳ Email Notifications (SendGrid)  
⏳ SMS Notifications (Twilio)  
⏳ Analytics & Reports (PowerBI)  

---

## 🎯 Conclusão

Este documento fornece uma **arquitetura completa e detalhada** para um sistema de leilão de alta performance usando:

- ✅ **DDD** com agregados bem definidos
- ✅ **Clean Architecture** com separação clara de camadas
- ✅ **Event-Driven Architecture** para desacoplamento
- ✅ **CQRS** para otimização de leitura/escrita
- ✅ **Concurrency Control** robusto com Kafka + Redis
- ✅ **Real-time Updates** via SignalR
- ✅ **Observability** com métricas, logs e tracing

O sistema está preparado para:
- 🚀 **3.000+ usuários simultâneos** por leilão
- ⚡ **Latência < 500ms** p95
- 🔒 **Ordem garantida** de lances
- 📈 **Escalabilidade horizontal**
- 🛡️ **Alta disponibilidade**

---

**Autor:** Sistema de Arquitetura de Software  
**Data:** 2025-01-21  
**Versão:** 1.0
