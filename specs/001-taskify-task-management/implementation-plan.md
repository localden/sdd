# Implementation Plan: Taskify Task Management Application

**Feature Branch**: `001-taskify-task-management`  
**Created**: 2025-07-19  
**Specification**: [feature-spec.md](./feature-spec.md)  

---

## ⚡ Quick Guidelines

**Note**: This document serves two purposes:
1. **As a template** - For AIs/humans creating implementation plans
2. **As a guide** - For AIs/humans executing the implementation

Instructions marked *(for execution)* apply when implementing the feature.
Instructions marked *(when creating this plan)* apply when filling out this template.

- ✅ Mark all technical decisions that need clarification
- ✅ Use [NEEDS CLARIFICATION: question] for any assumptions
- ❌ Don't guess at technical choices without context
- ❌ Don't include actual code - use pseudocode or references
- 📋 The review checklist acts as "unit tests" for this plan
- 📁 Extract details to `implementation-details/` files

---

## Executive Summary *(mandatory)*

Taskify will be implemented as a .NET Aspire solution using PostgreSQL for data persistence, Blazor Server for the interactive frontend with drag-and-drop kanban boards, and SignalR for real-time updates. The solution includes three REST APIs (Projects, Tasks, Notifications) and a responsive web interface optimized for product manager workflows.

## Requirements *(mandatory)*

**Minimum Versions**: .NET 9.0, PostgreSQL 15+, .NET Aspire 9.0  
**Dependencies**: 
- .NET Aspire (orchestration and service discovery)
- Entity Framework Core (PostgreSQL provider)
- SignalR (real-time updates)
- Blazor Server (UI framework)
- SMTP service for notifications
**Technology Stack**: PostgreSQL database, Redis cache (via Aspire), SMTP email service  
**Feature Spec Alignment**: [x] All requirements addressed

---

## Constitutional Compliance *(mandatory)*

*Note: The Constitution articles referenced below can be found in `/memory/constitution.md`. AI agents should read this file to understand the specific requirements of each article.*

### Simplicity Declaration (Articles VII & VIII)
- **Project Count**: 3 (Taskify.AppHost, Taskify.ApiService, Taskify.Web)
- **Model Strategy**: [x] Single model (shared models across API and UI)
- **Framework Usage**: [x] Direct (EF Core, Blazor Server, SignalR directly)
- **Patterns Used**: [x] None (using framework conventions)

### Testing Strategy (Articles III & IX)
- **Test Order**: Contract → Integration → E2E → Unit
- **Contract Location**: `/contracts/`
- **Real Environments**: [x] Yes (PostgreSQL container for integration tests)
- **Coverage Target**: 80% minimum, 100% critical paths (task operations, notifications)

### Library Organization (Articles I & II)
- **Libraries**: 
  - Taskify.ApiService: REST APIs for Projects, Tasks, Notifications
  - Taskify.Web: Blazor Server UI with drag-and-drop kanban boards
  - Taskify.AppHost: .NET Aspire orchestration
- **CLI Interfaces**: 
  - API service: health checks, database migration commands
  - Web app: health checks, user management commands
- **CLI Standards**: All CLIs implement --help, --version, --format
- **Inter-Library Contracts**: HTTP APIs between Web and ApiService

### Observability (Article V)
- [x] Structured logging planned (Serilog with .NET Aspire)
- [x] Error reporting defined (structured logging + health checks)
- [x] Metrics collection (.NET Aspire dashboard and telemetry)

---

## Project Structure *(mandatory)*

```
001-taskify-task-management/
├── implementation-plan.md              # This document (HIGH-LEVEL ONLY)
├── manual-testing.md                  # Step-by-step validation instructions
├── implementation-details/             # Detailed specifications
│   ├── 00-research.md                 # .NET Aspire and drag-drop research
│   ├── 01-environment-setup.md        # Development setup instructions
│   ├── 02-data-model.md               # EF Core entities and relationships
│   ├── 03-api-contracts.md            # OpenAPI specifications
│   ├── 04-algorithms.md               # Drag-drop state management
│   ├── 05-integrations.md             # SignalR and SMTP integration
│   ├── 06-contract-tests.md           # API test specifications
│   ├── 07-integration-tests.md        # End-to-end test scenarios
│   └── 08-inter-library-tests.md      # Web-to-API integration tests
├── contracts/                          # API contracts (FIRST)
│   ├── projects-api.json
│   ├── tasks-api.json
│   └── notifications-api.json
├── src/
│   ├── Taskify.AppHost/               # .NET Aspire orchestration
│   ├── Taskify.ApiService/            # REST APIs
│   └── Taskify.Web/                   # Blazor Server UI
└── tests/
    ├── contract/                       # Contract tests (FIRST)
    ├── integration/                    # Integration tests
    ├── inter-library/                  # Web-to-API tests
    └── unit/                          # Unit tests (LAST)
```

### File Creation Order
1. Create directory structure
2. Create `implementation-details/00-research.md` (Aspire setup and drag-drop libraries)
3. Create `contracts/` with OpenAPI specifications
4. Create `implementation-details/03-api-contracts.md`
5. Create test files in order: contract → integration → e2e → unit
6. Create source files to make tests pass
7. Create `manual-testing.md` for E2E validation

**IMPORTANT**: This implementation plan should remain high-level and readable. Any code samples, detailed algorithms, or extensive technical specifications must be placed in the appropriate `implementation-details/` file and referenced here.

---

## Implementation Phases *(mandatory)*

### Phase -1: Pre-Implementation Gates

#### Technical Unknowns
- [x] Complex areas identified: 
  - Blazor Server drag-and-drop implementation with real-time sync
  - .NET Aspire service discovery and configuration
  - SignalR hub design for task board updates
- [x] Research completed ✅ ALL RESEARCH COMPLETE (see implementation-details/00-research.md)

#### Simplicity Gate (Article VII)
- [x] Using ≤3 projects? (AppHost, ApiService, Web)
- [x] No future-proofing? (Building for current requirements only)
- [x] No unnecessary patterns? (Using EF Core and Blazor conventions)

#### Anti-Abstraction Gate (Article VIII)
- [x] Using framework directly? (EF Core, Blazor Server, SignalR)
- [x] Single model representation? (Shared models between API and UI)
- [x] Concrete classes by default? (No unnecessary interfaces)

#### Integration-First Gate (Article IX)
- [x] Contracts defined? (OpenAPI specs for all three APIs)
- [x] Contract tests written? (HTTP client tests against contracts)
- [x] Integration plan ready? (Web app calling API service via Aspire service discovery)

### Verification: Phase -1 Complete *(execution checkpoint)*
- [x] All gates passed or exceptions documented in Complexity Tracking
- [x] Research findings documented in implementation-details/00-research.md
- [x] Ready to create directory structure

### Phase 0: Contract & Test Setup

**Prerequisites** *(for execution)*: Phase -1 verification complete
**Deliverables** *(from execution)*: Failing contract tests, API specifications, test strategy

#### Step 0.1: Environment Setup
**Task**: Set up development environment and project structure
**Reference**: `implementation-details/01-environment-setup.md`
**Specific Actions**:
- Install .NET 9.0 SDK, Docker Desktop, IDE
- Create solution with 3 projects: AppHost, ApiService, Web
- Add NuGet package references for Aspire, EF Core, SignalR, Syncfusion
- Configure PostgreSQL and MailDev containers
- Verify services start via AppHost

#### Step 0.2: Define API Contracts
**Task**: Create OpenAPI specifications for three REST APIs
**Reference**: `implementation-details/03-api-contracts.md` (Section: "OpenAPI Specifications")
**Specific Actions**:
- Create `contracts/projects-api.json` with endpoints: GET/POST/PUT projects, POST members
- Create `contracts/tasks-api.json` with endpoints: GET project tasks, POST/PUT tasks, PUT move/assign
- Create `contracts/notifications-api.json` with endpoints: GET notifications, PUT mark read
- Define DTOs: ProjectDto, TaskDto, KanbanBoardDto, request/response schemas
- Validate JSON schemas using OpenAPI tools

#### Step 0.3: Write Contract Tests  
**Task**: Create failing HTTP client tests for each API endpoint
**Reference**: `implementation-details/06-contract-tests.md` (will create next)
**Specific Actions**:
- Set up `tests/contract/` with Aspire.Testing host
- Write HTTP client tests for each endpoint in contracts
- Test request/response schema validation
- Verify error response formats (400, 401, 404, 500)
- Ensure tests FAIL (no implementation yet)

#### Step 0.4: Design Integration Tests
**Task**: Plan comprehensive real-time workflow testing
**Reference**: `implementation-details/07-integration-tests.md` (will create next)
**Specific Actions**:
- Plan user workflow tests: create project → add tasks → drag-drop → real-time updates
- Design SignalR integration tests: task moves trigger broadcasts
- Plan Web-to-API integration via Aspire service discovery
- Define test data setup and cleanup strategies

#### Step 0.5: Create Manual Testing Guide
**Task**: Document step-by-step validation procedures
**Reference**: `manual-testing.md` (will create next)
**Specific Actions**:
- Map each user story from feature-spec.md to validation steps
- Document Aspire startup sequence and service health checks
- Create kanban board testing scenarios (drag-drop, real-time updates)
- Define browser testing matrix and mobile responsiveness checks

### Verification: Phase 0 Complete *(execution checkpoint)*
- [ ] API contracts exist in `/contracts/`
- [ ] Contract tests written and failing
- [ ] Integration test plan documented
- [ ] Manual testing guide created
- [ ] All detail files referenced exist

### Phase 1: Core Implementation

**Prerequisites** *(for execution)*: Phase 0 verification complete
**Deliverables** *(from execution)*: Working implementation passing all contract tests

#### Step 1.1: Database Models and Migration
**Task**: Implement EF Core entities and database schema
**Reference**: `implementation-details/02-data-model.md`
**Specific Actions**:
- Create entities: User, Project, ProjectMember, TaskItem, Notification
- Configure TaskifyDbContext with relationships and indexes
- Set up database seeding with sample data
- Create and run EF Core migration: `dotnet ef migrations add InitialCreate`
- Verify database schema matches design in pgAdmin

#### Step 1.2: API Service Implementation
**Task**: Build REST API controllers with authentication and SignalR
**Reference**: `implementation-details/03-api-contracts.md` (Section: "Controller Implementation")
**Reference**: `implementation-details/05-integrations.md` (Section: "SignalR Hubs")
**Specific Actions**:
- Implement ProjectsController with CRUD operations and member management
- Implement TasksController with kanban operations and real-time updates
- Implement NotificationsController with pagination and read status
- Create TaskNotificationHub for SignalR broadcasts
- Configure JWT authentication and authorization policies
- Add Aspire service defaults and health checks

#### Step 1.3: Blazor Server Frontend
**Task**: Build interactive kanban board with real-time features
**Reference**: `implementation-details/04-algorithms.md` (will create with Syncfusion patterns)
**Reference**: Research findings in `00-research.md` (Syncfusion integration section)
**Specific Actions**:
- Create Syncfusion kanban board component with drag-and-drop
- Implement SignalR client service for real-time updates
- Build authentication pages and user management
- Add responsive CSS for mobile/tablet support
- Integrate with API service via HttpClient and service discovery

#### Step 1.4: Real-Time Integration
**Task**: Connect all components for seamless real-time experience
**Reference**: `implementation-details/05-integrations.md` (will create comprehensive integration guide)
**Reference**: Research findings in `00-research.md` (Complete real-time flow section)
**Specific Actions**:
- Implement complete drag-drop → API → SignalR → UI update flow
- Configure Aspire AppHost for service discovery between Web and API
- Set up authentication token passing for cross-service calls
- Implement email notifications triggered by task assignments
- Add error handling and connection resilience for SignalR

#### Step 1.5: Background Services
**Task**: Implement email notifications and worker services
**Reference**: Research findings in `00-research.md` (SMTP email notifications section)
**Specific Actions**:
- Create background email service with retry logic
- Implement database migration worker service
- Configure SMTP settings for development (MailDev) and production
- Set up proper service dependencies in AppHost (migrations → API → Web)

### Phase 2: Refinement

**Prerequisites** *(for execution)*: Phase 1 complete, all contract/integration tests passing
**Deliverables** *(from execution)*: Production-ready code with full test coverage

1. **Unit Tests** (for complex drag-drop logic and notification rules)
2. **Performance Optimization** (query optimization, SignalR connection management)
3. **Documentation Updates** (README, API documentation)
4. **Manual Testing Execution**
   - Follow manual-testing.md procedures
   - Verify drag-and-drop works across browsers
   - Test real-time updates with multiple users
   - Validate email notifications

### Verification: Phase 2 Complete *(execution checkpoint)*
- [ ] All tests passing (contract, integration, unit)
- [ ] Manual testing completed successfully
- [ ] Performance metrics meet requirements (1-2 second response times)
- [ ] Documentation updated

---

## Success Criteria *(mandatory)*

1. **Constitutional**: All gates passed with no unjustified complexity
2. **Functional**: All user stories work end-to-end with drag-and-drop kanban boards
3. **Testing**: Contract/Integration tests comprehensive with real PostgreSQL
4. **Performance**: Task operations complete within 1 second, real-time updates immediate
5. **Simplicity**: Three projects only, direct framework usage, single model representation

---

## Review & Acceptance Checklist

### Plan Completeness
- [x] No [NEEDS CLARIFICATION] markers remain
- [x] All mandatory sections completed
- [x] Technology stack fully specified (.NET Aspire, PostgreSQL, Blazor Server, SignalR)
- [x] Dependencies justified (all are framework components or required for features)

### Constitutional Alignment
- [x] All Phase -1 gates passed with no exceptions needed
- [x] No deviations requiring Complexity Tracking section

### Technical Readiness
- [x] Phase 0 verification checklist defined
- [x] Phase 1 implementation path clear
- [x] Success criteria measurable

### Risk Management
- [x] Complex areas identified (drag-drop, real-time updates, Aspire configuration)
- [x] Integration points clearly defined (Web-to-API, SignalR, SMTP)
- [x] Performance requirements specified (1-2 second response times)
- [x] Security considerations addressed (authentication, data encryption)

### Implementation Clarity
- [x] All phases have clear prerequisites and deliverables
- [x] No speculative or "might need" features
- [x] Manual testing procedures defined for kanban boards

---

*This plan follows Constitution v2.0.0 (see `/memory/constitution.md`) emphasizing simplicity, framework trust, and integration-first testing.*