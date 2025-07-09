# Manual Testing Guide: Taskify

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK installed
- Docker Desktop running
- Visual Studio 2022 or VS Code

### Build and Run
1. **Clone and navigate to project:**
   ```bash
   git checkout 001-create-taskify
   cd specs/001-create-taskify/src
   ```

2. **Start infrastructure:**
   ```bash
   docker-compose up -d postgres
   ```

3. **Build and run application:**
   ```bash
   dotnet run --project Taskify.AppHost
   ```

4. **Verify services are running:**
   - API: https://localhost:7001/swagger
   - Web: https://localhost:7000
   - PostgreSQL: localhost:5432

### Test Data Setup
1. **Seed database with sample data:**
   ```bash
   dotnet run --project Taskify.Api -- --seed-data
   ```

2. **Verify data seeded:**
   - 5 users created
   - 3 projects created
   - 61 tasks distributed across projects
   - Sample comments added

---

## User Story Validation

### US-001: User Selection and Identity
**Story**: As a team member, I want to select my identity from a predefined list so that I can access the platform and see my assigned tasks.

#### Test Steps:
1. **Navigate to application**: Open https://localhost:7000
2. **Verify user selection screen**: 
   - Should display 5 users:
     - Sarah Chen (Product Manager)
     - Alex Rodriguez (Senior Engineer)
     - Jordan Kim (Engineer)
     - Taylor Swift (Engineer)
     - Morgan Davis (Engineer)
3. **Select a user**: Click on "Sarah Chen"
4. **Verify user persistence**: 
   - User name should appear in header
   - Navigate to different pages, user should remain selected
5. **Verify task highlighting**: 
   - Tasks assigned to Sarah Chen should be highlighted with distinct color
   - Other users' tasks should have normal appearance

#### Expected Results:
- [ ] User selection screen displays all 5 users
- [ ] User selection persists throughout session
- [ ] Assigned tasks are visually distinct from other tasks
- [ ] No errors or page refreshes during user selection

#### Edge Cases:
- **Multiple users on same device**: Clear user selection and choose different user
- **Browser refresh**: User selection should persist after refresh

---

### US-002: Drag-and-Drop Task Management
**Story**: As a team member, I want to drag tasks between Kanban columns so that I can update task status efficiently.

#### Test Steps:
1. **Navigate to project**: Select "Mobile App Redesign" project
2. **Verify Kanban board**: 
   - Should display 4 columns: To Do, In Progress, In Review, Done
   - Should show tasks in each column
3. **Drag task between columns**: 
   - Find task in "To Do" column
   - Drag task to "In Progress" column
   - Release task
4. **Verify immediate UI update**: 
   - Task should appear in "In Progress" column
   - Task should disappear from "To Do" column
   - Task counts should update
5. **Verify persistence**: 
   - Refresh page
   - Task should remain in "In Progress" column
6. **Test all column combinations**: 
   - Drag task from "In Progress" to "In Review"
   - Drag task from "In Review" to "Done"
   - Drag task from "Done" back to "To Do"

#### Expected Results:
- [ ] Drag operation is smooth and responsive (<100ms)
- [ ] Task moves between columns immediately
- [ ] Task counts update automatically
- [ ] Changes persist after page refresh
- [ ] All column combinations work correctly

#### Edge Cases:
- **Network interruption**: Simulate network disconnection during drag
- **Concurrent updates**: Have multiple users drag tasks simultaneously
- **Invalid drops**: Try dropping task outside valid drop zones

---

### US-003: Task Commenting System
**Story**: As a team member, I want to comment on tasks so that I can collaborate with my team on specific work items.

#### Test Steps:
1. **Open task details**: Click on any task to open details modal
2. **Verify existing comments**: 
   - Should display existing comments with author names
   - Should display timestamps
3. **Add new comment**: 
   - Enter text in comment input field
   - Click "Add Comment" button
4. **Verify comment appears**: 
   - Comment should appear immediately
   - Should show correct author (current user)
   - Should show current timestamp
5. **Test comment editing**: 
   - Click "Edit" on your own comment
   - Modify comment text
   - Save changes
6. **Test comment deletion**: 
   - Click "Delete" on your own comment
   - Confirm deletion
   - Comment should disappear
7. **Test permission restrictions**: 
   - Switch to different user
   - Try to edit/delete other user's comments
   - Should be unable to modify others' comments

#### Expected Results:
- [ ] Comments display with author and timestamp
- [ ] New comments appear immediately
- [ ] User can edit/delete only their own comments
- [ ] Comment threading works correctly
- [ ] Real-time updates when others add comments

#### Edge Cases:
- **Long comments**: Test with very long comment text
- **Special characters**: Test with emoji, HTML tags, special characters
- **Concurrent comments**: Multiple users commenting simultaneously

---

### US-004: Task Reassignment
**Story**: As a team member, I want to reassign tasks to other team members so that I can balance workload.

#### Test Steps:
1. **Open task details**: Click on any task
2. **Verify current assignee**: Should display current assigned user
3. **Change assignment**: 
   - Click on assignee dropdown
   - Select different user from list
   - Save changes
4. **Verify immediate update**: 
   - Task should show new assignee
   - Task color should change to new user's color
5. **Verify in Kanban view**: 
   - Return to Kanban board
   - Task should reflect new assignment
6. **Test assignment to different users**: 
   - Assign tasks to each of the 5 users
   - Verify each assignment works correctly

#### Expected Results:
- [ ] Assignee dropdown shows all 5 users
- [ ] Assignment changes immediately
- [ ] Task color updates to reflect new assignee
- [ ] Changes persist after page refresh
- [ ] All users can be assigned tasks

---

### US-005: Project Overview with Task Counts
**Story**: As a team member, I want to see project overview with task counts so that I can understand project status at a glance.

#### Test Steps:
1. **Navigate to dashboard**: Go to main dashboard page
2. **Verify project list**: 
   - Should display all 3 projects
   - Should show task counts per column for each project
3. **Verify task count accuracy**: 
   - Mobile App Redesign: 6 ToDo, 4 InProgress, 3 InReview, 8 Done
   - API Integration Platform: 4 ToDo, 2 InProgress, 2 InReview, 12 Done
   - Team Onboarding System: 2 ToDo, 1 InProgress, 1 InReview, 16 Done
4. **Test count updates**: 
   - Move task between columns in any project
   - Return to dashboard
   - Task counts should update automatically
5. **Test real-time updates**: 
   - Have another user move tasks
   - Dashboard should update without refresh

#### Expected Results:
- [ ] All 3 projects display with correct titles
- [ ] Task counts are accurate per specification
- [ ] Counts update when tasks are moved
- [ ] Real-time updates work correctly
- [ ] Project loading completes within 2 seconds

---

### US-006: Different Project Activity Levels
**Story**: As a team member, I want to explore different project types and activity levels so that I can understand various workflow scenarios.

#### Test Steps:
1. **Explore Mobile App Redesign (High Activity)**:
   - Navigate to project
   - Verify many tasks in "To Do" and "In Progress"
   - Verify active development stage
2. **Explore API Integration Platform (Moderate Activity)**:
   - Navigate to project
   - Verify balanced distribution across columns
   - Verify moderate completion level
3. **Explore Team Onboarding System (Low Activity)**:
   - Navigate to project
   - Verify most tasks in "Done" column
   - Verify maintenance phase characteristics
4. **Compare project patterns**: 
   - Note different task distributions
   - Observe different completion percentages
   - Verify realistic workflow scenarios

#### Expected Results:
- [ ] Mobile App Redesign shows high activity pattern
- [ ] API Integration Platform shows moderate activity
- [ ] Team Onboarding System shows low activity/maintenance
- [ ] Each project demonstrates distinct workflow stage
- [ ] Task distributions are realistic and varied

---

## Performance Testing

### Drag-and-Drop Performance
**Requirement**: Task drag operations complete within 100ms

#### Test Steps:
1. **Measure drag response time**: 
   - Open browser developer tools
   - Navigate to Network tab
   - Drag task between columns
   - Measure API response time
2. **Test with multiple tasks**: 
   - Drag multiple tasks in quick succession
   - Verify each operation completes quickly
3. **Test under load**: 
   - Open multiple browser tabs
   - Perform simultaneous drag operations
   - Verify performance remains acceptable

#### Expected Results:
- [ ] Individual drag operations complete within 100ms
- [ ] Multiple operations maintain performance
- [ ] UI remains responsive during operations

### Project Loading Performance
**Requirement**: Project loading completes within 2 seconds

#### Test Steps:
1. **Measure project load time**: 
   - Clear browser cache
   - Navigate to project page
   - Measure load time from navigation to full render
2. **Test all projects**: 
   - Load each of the 3 projects
   - Verify all meet performance requirements
3. **Test with slow network**: 
   - Throttle network speed in developer tools
   - Verify reasonable performance

#### Expected Results:
- [ ] Project pages load within 2 seconds
- [ ] All projects meet performance targets
- [ ] Performance acceptable on slower networks

---

## Error Handling Testing

### Network Interruption
#### Test Steps:
1. **Simulate network disconnection**: 
   - Disconnect network during drag operation
   - Reconnect network
   - Verify operation completes or fails gracefully
2. **Test SignalR reconnection**: 
   - Disconnect network for 30 seconds
   - Reconnect network
   - Verify real-time updates resume

#### Expected Results:
- [ ] Operations fail gracefully with user feedback
- [ ] SignalR reconnects automatically
- [ ] No data loss during network interruption

### Invalid Operations
#### Test Steps:
1. **Test invalid task status**: 
   - Attempt to move task to invalid status
   - Verify appropriate error message
2. **Test non-existent resources**: 
   - Try to access non-existent task/project
   - Verify 404 error handling
3. **Test permission violations**: 
   - Try to modify others' comments
   - Verify access denied appropriately

#### Expected Results:
- [ ] Invalid operations show user-friendly errors
- [ ] Error messages are clear and actionable
- [ ] No system crashes or exceptions

---

## Multi-User Testing

### Concurrent Operations
**Requirement**: Support 5 concurrent users

#### Test Steps:
1. **Setup multiple users**: 
   - Open 5 browser windows
   - Select different user in each window
   - Navigate to same project
2. **Test concurrent task updates**: 
   - Have all users drag tasks simultaneously
   - Verify all operations complete successfully
3. **Test real-time notifications**: 
   - Have one user move task
   - Verify other users see update immediately
4. **Test concurrent commenting**: 
   - Have multiple users comment on same task
   - Verify all comments appear for all users

#### Expected Results:
- [ ] System handles 5 concurrent users
- [ ] No conflicts or data corruption
- [ ] Real-time updates work for all users
- [ ] Performance remains acceptable

---

## Browser Compatibility

### Cross-Browser Testing
#### Test Steps:
1. **Test in Chrome**: 
   - Complete full user story validation
   - Verify all features work correctly
2. **Test in Firefox**: 
   - Complete full user story validation
   - Verify all features work correctly
3. **Test in Safari**: 
   - Complete full user story validation
   - Verify all features work correctly
4. **Test in Edge**: 
   - Complete full user story validation
   - Verify all features work correctly

#### Expected Results:
- [ ] All features work in Chrome
- [ ] All features work in Firefox
- [ ] All features work in Safari
- [ ] All features work in Edge
- [ ] Consistent behavior across browsers

---

## Data Integrity Testing

### Persistent State
#### Test Steps:
1. **Test session persistence**: 
   - Make changes to tasks
   - Refresh browser
   - Verify changes persist
2. **Test data consistency**: 
   - Move tasks between columns
   - Verify task counts update correctly
   - Check database state directly
3. **Test comment persistence**: 
   - Add comments to tasks
   - Refresh browser
   - Verify comments remain

#### Expected Results:
- [ ] All changes persist after browser refresh
- [ ] Task counts remain consistent
- [ ] Comments are preserved correctly
- [ ] No data loss under normal conditions

---

## Cleanup

### Post-Testing Cleanup
1. **Stop services**: 
   ```bash
   docker-compose down
   ```
2. **Clean up test data**: 
   ```bash
   dotnet run --project Taskify.Api -- --reset-database
   ```
3. **Verify clean state**: 
   - Restart application
   - Verify fresh state

---

## Issue Reporting

### When Issues Are Found
1. **Document the issue**: 
   - User story affected
   - Steps to reproduce
   - Expected vs actual behavior
   - Browser/environment details
2. **Categorize severity**: 
   - Critical: Feature completely broken
   - High: Feature works but major usability issue
   - Medium: Minor usability issue
   - Low: Cosmetic issue
3. **Report to development team**: 
   - Include all documentation
   - Provide screenshots/videos if helpful
   - Suggest workarounds if available

### Success Criteria
All user stories must pass validation with:
- [ ] No critical or high severity issues
- [ ] Performance requirements met
- [ ] Cross-browser compatibility confirmed
- [ ] Multi-user functionality working
- [ ] Data integrity maintained