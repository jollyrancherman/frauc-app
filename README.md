# Frauc - Marketplace Auction Platform

A Facebook Marketplace-style auction platform built with modern microservices architecture, supporting both forward auctions (highest bidder wins) and reverse auctions (lowest bidder wins), along with fixed-price listings.

## 🚀 Features

- **Real-time Bidding**: Live auction updates using SignalR
- **Social Authentication**: Keycloak integration with Google, Facebook, Apple login
- **Geospatial Search**: Location-based listings with PostGIS
- **Payment Processing**: Secure transactions via Stripe with escrow system
- **Messaging System**: Real-time buyer-seller communication
- **Advanced Search**: Elasticsearch-powered search with filters
- **Monetization**: Dynamic commission fees, advertising platform
- **Mobile-First**: Responsive design for all devices

## 🏗️ Architecture

### Tech Stack
- **Backend**: .NET 9 microservices with Domain-Driven Design (DDD)
- **Frontend**: NextJS 15 with App Router
- **Authentication**: Keycloak with social login integration
- **Database**: PostgreSQL 17 with PostGIS for geospatial features
- **Real-time**: SignalR for live bidding and messaging
- **Caching**: Redis
- **Search**: Elasticsearch
- **Testing**: 100% test coverage using TDD methodology

### Microservices
- User Service (profiles, ratings, verification)
- Product Service (catalog, categories, specifications)
- Listing Service (auctions, fixed-price listings)
- Bidding Service (bid management, auto-bidding, proxy bidding)
- Payment Service (Stripe integration, escrow, commission tracking)
- Messaging Service (SignalR real-time chat)
- Geolocation Service (PostGIS spatial queries)
- Search Service (Elasticsearch integration)
- Analytics Service (business intelligence, user behavior)
- Notification Service (real-time alerts, email/SMS)
- File Storage Service (Azure Blob + CDN)
- Dispute Service (resolution workflows)
- Advertising Service (AdSense + native ads)
- Admin Service (content moderation, platform management)
- Revenue Service (monetization, subscription management)

## 🚦 Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- Docker & Docker Compose
- Git

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/jollyrancherman/frauc-app.git
   cd frauc-app
   ```

2. **Start the development environment**
   ```bash
   docker-compose -f infrastructure/docker/docker-compose.dev.yml up -d
   ```

3. **Backend Development**
   ```bash
   cd backend/
   dotnet build
   dotnet watch --project src/Services/User.API
   ```

4. **Frontend Development**
   ```bash
   cd frontend/
   npm ci
   npm run dev
   ```

### Development Environment

The Docker Compose setup includes:
- **Keycloak** (port 8080) - Authentication server
- **PostgreSQL 17** (port 5432) - Main database with PostGIS
- **Redis** (port 6379) - Caching and SignalR backplane
- **Elasticsearch** (port 9200) - Search functionality

## 🧪 Testing

This project follows strict Test-Driven Development (TDD) with 100% test coverage requirement.

### Running Tests

**Backend Tests:**
```bash
cd backend/
dotnet test --collect:"XPlat Code Coverage"
```

**Frontend Tests:**
```bash
cd frontend/
npm run test -- --coverage --watchAll=false
```

**End-to-End Tests:**
```bash
cd frontend/
npx playwright test
```

### Test Coverage
- **Unit Tests**: 70% of total tests (xUnit, Jest)
- **Integration Tests**: 20% of total tests (TestContainers, MSW)
- **E2E Tests**: 10% of total tests (Playwright)
- **Minimum Coverage**: 100% line coverage across all services

## 📊 Development Progress

**Current Status**: Week 1 - Authentication & User Management
- ✅ Infrastructure & TDD Setup (Days 1-2) - **COMPLETED**
- ⏳ User Profile Service (Days 3-5) - **IN PROGRESS**

See [DEVELOPMENT_PLAN.md](./DEVELOPMENT_PLAN.md) for the complete 14-week development roadmap.

## 🔧 Development Commands

### Backend (.NET 9)
```bash
cd backend/

# Build all services
dotnet build

# Run specific service with hot reload
dotnet watch --project src/Services/User.API

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Database migrations
dotnet ef migrations add <MigrationName> --project src/Services/User.API
dotnet ef database update --project src/Services/User.API
```

### Frontend (NextJS 15)
```bash
cd frontend/

# Install dependencies
npm ci

# Run development server
npm run dev

# Build for production
npm run build

# Run tests with coverage
npm run test -- --coverage --watchAll=false

# Run E2E tests
npx playwright test
```

### Infrastructure
```bash
# Start local development environment
docker-compose -f infrastructure/docker/docker-compose.dev.yml up -d

# Start test environment
docker-compose -f infrastructure/docker/docker-compose.test.yml up -d

# View logs
docker-compose -f infrastructure/docker/docker-compose.dev.yml logs -f [service-name]
```

## 📁 Repository Structure

```
/
├── backend/                # .NET 9 microservices
│   ├── src/               # Source code
│   │   ├── Services/      # Individual microservices
│   │   ├── Shared/        # Shared libraries
│   │   └── ApiGateway/    # API gateway
│   ├── tests/             # Test projects
│   └── Marketplace.sln
│
├── frontend/              # NextJS 15 web app
│   ├── src/              # Source code
│   ├── tests/            # Test files
│   └── package.json
│
├── mobile/               # React Native app (future)
│
├── infrastructure/       # Infrastructure as Code
│   ├── docker/          # Docker configurations
│   ├── kubernetes/      # K8s manifests
│   └── scripts/         # Utility scripts
│
└── shared/              # Cross-platform shared code
    ├── contracts/       # API contracts
    └── types/          # TypeScript definitions
```

## 🔐 Security

- JWT Bearer tokens from Keycloak
- Role-based access control (RBAC)
- Rate limiting per user and IP
- OWASP security best practices
- Encryption at rest and in transit
- GDPR compliance with data export/deletion

## 📈 Monitoring & Performance

- Application Insights for .NET services
- Prometheus + Grafana for custom metrics
- ELK Stack for centralized logging
- Real-time business metrics tracking
- Redis distributed caching
- CDN for static assets

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Write tests first (TDD approach)
4. Implement the feature
5. Ensure 100% test coverage
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## 📋 Development Workflow

1. **Start with failing test (RED)**
2. **Implement minimal functionality (GREEN)**
3. **Refactor while maintaining tests (REFACTOR)**
4. **Ensure 100% coverage before commit**
5. **Run integration tests locally**
6. **Submit PR with coverage report**

## 📚 Documentation

- [DEVELOPMENT_PLAN.md](./DEVELOPMENT_PLAN.md) - Complete 14-week development roadmap
- [CLAUDE.md](./CLAUDE.md) - Claude Code integration and project instructions

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🚀 Deployment

The platform is designed for Kubernetes deployment with:
- Blue-green deployment strategy
- Automatic scaling based on metrics
- Zero-downtime migrations
- Comprehensive health checks

For production deployment instructions, see the `infrastructure/kubernetes/` directory.

---

**Built with ❤️ using Test-Driven Development and 100% test coverage**