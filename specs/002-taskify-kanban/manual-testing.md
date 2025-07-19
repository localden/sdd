# Manual Testing Guide: Enhanced Kanban Board System

**Document**: 002-taskify-kanban/manual-testing.md  
**Created**: 2025-07-19  
**Status**: Comprehensive manual validation procedures  

---

## Testing Overview

This guide provides step-by-step manual testing procedures to validate all enhanced kanban board functionality. Follow these tests to ensure the system meets all requirements before production deployment.

### Testing Environment Requirements
- **Browsers**: Chrome, Firefox, Safari, Edge (latest versions)
- **Mobile Devices**: iOS (iPhone/iPad), Android (phone/tablet)
- **Screen Resolutions**: 1920x1080, 1366x768, 375x667 (mobile)
- **Network Conditions**: High-speed, 3G simulation, offline scenarios
- **Multiple Users**: At least 3 concurrent test accounts

### Pre-Test Setup
1. Start the Aspire application: `dotnet run --project src/Taskify.AppHost`
2. Verify all services are healthy: Check https://localhost:15888 dashboard
3. Create test users and projects via API or admin interface
4. Have multiple browser instances/devices ready for collaboration testing

---

## Test Suite 1: Board Management

### Test 1.1: Create Kanban Board
**Objective**: Verify board creation with custom configuration

**Steps**:
1. Navigate to Projects page
2. Select an existing project or create new project
3. Click "Create Kanban Board"
4. Fill out board details:
   - Name: "Marketing Campaign Board"
   - Description: "Track marketing campaign tasks"
   - Enable WIP limits: ✓
   - Enable swimlanes: ✓
   - Default swimlane: "Assignee"
5. Configure columns:
   - "Backlog" (WIP: unlimited)
   - "To Do" (WIP: 5)
   - "In Progress" (WIP: 3)
   - "Review" (WIP: 2)
   - "Done" (WIP: unlimited)
6. Click "Create Board"

**Expected Results**:
- ✅ Board created successfully
- ✅ All columns appear in correct order
- ✅ WIP limits displayed on columns
- ✅ Board settings saved correctly
- ✅ Redirect to new board view

**Validation**:
- Verify board appears in project's board list
- Check database for correct board and column records
- Confirm all settings match configuration

### Test 1.2: Configure Board Settings
**Objective**: Verify board configuration updates work correctly

**Steps**:
1. Open existing kanban board
2. Click "Board Settings" (gear icon)
3. Modify settings:
   - Change name to "Updated Campaign Board"
   - Disable WIP limits
   - Change default swimlane to "Priority"
   - Update theme to "Dark"
4. Click "Save Changes"
5. Verify changes applied immediately

**Expected Results**:
- ✅ Board name updated in header
- ✅ WIP limit indicators removed from columns
- ✅ Swimlane organization changes
- ✅ Visual theme updates
- ✅ Settings persist after page refresh

### Test 1.3: Manage Board Columns
**Objective**: Verify column management functionality

**Steps**:
1. Open board settings
2. Add new column:
   - Name: "Testing"
   - Position: Between "Review" and "Done"
   - WIP Limit: 1
   - Color: Orange
3. Edit existing column:
   - Change "In Progress" WIP limit from 3 to 4
   - Change color to blue
4. Reorder columns by dragging
5. Delete "Backlog" column (if empty)
6. Save changes

**Expected Results**:
- ✅ New column appears in correct position
- ✅ Column properties updated correctly
- ✅ Drag-and-drop reordering works
- ✅ Column deletion works when empty
- ✅ Cannot delete column with tasks
- ✅ Changes reflect immediately on board

---

## Test Suite 2: Task Management and Movement

### Test 2.1: Basic Task Operations
**Objective**: Verify core task operations on kanban board

**Steps**:
1. Create new task directly on board:
   - Click "+" in "To Do" column
   - Title: "Design campaign mockups"
   - Description: "Create initial design concepts"
   - Assignee: Test User 1
   - Priority: High
   - Due Date: Tomorrow
2. Edit task inline:
   - Double-click task title
   - Change to "Design campaign mockups v2"
   - Press Enter
3. View task details:
   - Click on task card
   - Verify all information displays correctly
   - Add tags: "design", "urgent"
4. Move task between columns by dragging

**Expected Results**:
- ✅ Task creation works from board
- ✅ Inline editing functions properly
- ✅ Task details modal displays correctly
- ✅ Tags can be added and displayed
- ✅ Drag-and-drop movement works smoothly

### Test 2.2: Drag-and-Drop Task Movement
**Objective**: Verify advanced drag-and-drop functionality

**Steps**:
1. Create 5 tasks in "To Do" column
2. Drag first task to "In Progress":
   - Observe visual feedback during drag
   - Drop in specific position (middle of column)
   - Verify task moves to correct position
3. Drag task from "In Progress" to "Review":
   - Drag over column header
   - Observe column highlighting
   - Drop task
4. Try invalid move:
   - Drag task to column at WIP limit
   - Observe rejection behavior
5. Batch move multiple tasks:
   - Select multiple tasks (Ctrl+click)
   - Drag selection to new column

**Expected Results**:
- ✅ Smooth drag animation and visual feedback
- ✅ Drop zones highlighted appropriately
- ✅ Tasks positioned correctly after drop
- ✅ WIP limit violations prevented with clear feedback
- ✅ Multi-select and batch operations work
- ✅ Position maintained within columns

### Test 2.3: WIP Limit Enforcement
**Objective**: Verify Work-in-Progress limits function correctly

**Steps**:
1. Set "In Progress" column WIP limit to 2
2. Move 2 tasks to "In Progress" column
3. Attempt to move 3rd task:
   - Drag task over "In Progress" column
   - Observe visual warning
   - Try to drop task
   - Verify rejection
4. Complete one task (move to "Done")
5. Retry moving new task to "In Progress"
6. Test WIP limit warning indicators:
   - When column reaches 80% capacity
   - When column reaches 100% capacity

**Expected Results**:
- ✅ Column shows task count vs. limit (2/2)
- ✅ Visual warning when approaching limit
- ✅ Clear rejection when limit exceeded
- ✅ Helpful error message displayed
- ✅ Limit enforcement only when enabled
- ✅ Can move tasks out of full column

---

## Test Suite 3: Real-Time Collaboration

### Test 3.1: Multi-User Task Movement
**Objective**: Verify real-time updates work across multiple users

**Preparation**:
- Open same board in 2 different browsers/devices
- Login as different users in each browser

**Steps**:
1. **Browser 1**: Create new task "Collaboration Test"
2. **Browser 2**: Verify task appears immediately
3. **Browser 1**: Move task from "To Do" to "In Progress"
4. **Browser 2**: Observe real-time movement
5. **Browser 2**: Edit task title to "Collaboration Test - Updated"
6. **Browser 1**: Verify title update appears
7. **Both browsers**: Attempt to move same task simultaneously
8. **Both browsers**: Observe conflict resolution

**Expected Results**:
- ✅ Task creation appears in < 500ms
- ✅ Task movements update in < 200ms
- ✅ Task edits synchronize immediately
- ✅ Conflict resolution works properly
- ✅ No data loss during conflicts
- ✅ User presence indicators visible

### Test 3.2: Collaborative Drag Operations
**Objective**: Verify real-time drag feedback between users

**Steps**:
1. **Browser 1**: Start dragging a task (don't drop)
2. **Browser 2**: Observe task appears as "being moved"
3. **Browser 2**: Try to interact with same task
4. **Browser 1**: Move task through different columns
5. **Browser 2**: Watch real-time drag preview
6. **Browser 1**: Drop task in final position
7. **Browser 2**: Verify final position updates
8. **Browser 1**: Cancel drag operation (press Escape)
9. **Browser 2**: Verify task returns to original position

**Expected Results**:
- ✅ Drag state visible to other users
- ✅ Task locked during drag by another user
- ✅ Real-time drag preview updates
- ✅ Proper handling of drag cancellation
- ✅ Clear visual indicators for locked tasks

### Test 3.3: User Presence and Activity
**Objective**: Verify user presence features work correctly

**Steps**:
1. **Browser 1**: Join board
2. **Browser 2**: Join same board
3. **Both browsers**: Verify user presence list shows both users
4. **Browser 1**: Start typing in task description
5. **Browser 2**: Observe typing indicator
6. **Browser 1**: Go idle (no activity for 2 minutes)
7. **Browser 2**: Verify user status changes to "idle"
8. **Browser 1**: Close browser
9. **Browser 2**: Verify user appears as "offline"

**Expected Results**:
- ✅ Active users list displays correctly
- ✅ Typing indicators appear in real-time
- ✅ User status updates (active/idle/offline)
- ✅ Presence information accurate
- ✅ Clean user removal on disconnect

---

## Test Suite 4: Swimlane Organization

### Test 4.1: Assignee-Based Swimlanes
**Objective**: Verify swimlane organization by assignee

**Steps**:
1. Enable swimlanes in board settings
2. Set default organization to "Assignee"
3. Create tasks assigned to different users:
   - 2 tasks assigned to User A
   - 2 tasks assigned to User B
   - 1 task unassigned
4. Verify swimlane organization:
   - User A swimlane with 2 tasks
   - User B swimlane with 2 tasks
   - "Unassigned" swimlane with 1 task
5. Move task between swimlanes by changing assignee
6. Drag task from one swimlane to another

**Expected Results**:
- ✅ Swimlanes appear as horizontal rows
- ✅ Tasks grouped correctly by assignee
- ✅ Unassigned tasks have dedicated swimlane
- ✅ Assignee changes update swimlane placement
- ✅ Cross-swimlane dragging works
- ✅ Swimlane headers show assignee names/avatars

### Test 4.2: Priority-Based Swimlanes
**Objective**: Verify swimlane organization by priority

**Steps**:
1. Change swimlane organization to "Priority"
2. Create tasks with different priorities:
   - 2 High priority tasks
   - 3 Medium priority tasks  
   - 2 Low priority tasks
3. Verify priority swimlanes:
   - High priority swimlane at top
   - Medium priority in middle
   - Low priority at bottom
4. Change task priority and observe movement
5. Filter by specific priority levels

**Expected Results**:
- ✅ Swimlanes ordered by priority (High → Medium → Low)
- ✅ Tasks grouped correctly by priority
- ✅ Priority changes trigger swimlane movement
- ✅ Color coding matches priority levels
- ✅ Filtering works with swimlanes

### Test 4.3: Custom Swimlane Values
**Objective**: Verify custom swimlane configurations

**Steps**:
1. Set swimlane organization to "Custom"
2. Create custom swimlane values:
   - "Sprint 1"
   - "Sprint 2"
   - "Bug Fixes"
3. Assign tasks to custom swimlanes
4. Verify grouping and organization
5. Add new custom swimlane dynamically
6. Remove empty custom swimlane

**Expected Results**:
- ✅ Custom swimlanes display correctly
- ✅ Tasks grouped by custom values
- ✅ Dynamic swimlane creation/removal
- ✅ Custom values persist across sessions

---

## Test Suite 5: Filtering and Search

### Test 5.1: Basic Filtering
**Objective**: Verify board filtering functionality

**Steps**:
1. Create diverse set of tasks:
   - Various assignees, priorities, statuses
   - Different due dates (overdue, today, future)
   - Multiple tags ("bug", "feature", "urgent")
2. Test individual filters:
   - Filter by assignee: Select specific user
   - Filter by priority: Show only "High" priority
   - Filter by status: Show only "In Progress"
   - Filter by due date: Show overdue tasks only
   - Filter by tags: Show tasks with "urgent" tag
3. Clear filters and verify all tasks return
4. Save filter as "My High Priority Tasks"

**Expected Results**:
- ✅ Each filter works independently
- ✅ Filtered tasks display immediately
- ✅ Task count updates correctly
- ✅ Clear filters restores full view
- ✅ Saved filters persist

### Test 5.2: Combined Filtering
**Objective**: Verify multiple filters work together

**Steps**:
1. Apply multiple filters simultaneously:
   - Assignee: Current user
   - Priority: High + Medium
   - Status: To Do + In Progress
   - Tags: Contains "urgent"
2. Verify only tasks matching ALL criteria display
3. Remove one filter at a time
4. Observe results update correctly
5. Apply contradictory filters (verify empty result)

**Expected Results**:
- ✅ Multiple filters combine with AND logic
- ✅ Results update dynamically
- ✅ Empty results handled gracefully
- ✅ Filter combination logic clear to user

### Test 5.3: Search Functionality
**Objective**: Verify text search across task content

**Steps**:
1. Create tasks with searchable content:
   - Titles: "Design homepage", "Fix login bug"
   - Descriptions: Various detailed descriptions
2. Test search queries:
   - "homepage" → Should find design task
   - "login" → Should find bug task
   - "bug" → Should find multiple tasks
   - "xyz123" → Should return no results
3. Test search with filters applied
4. Clear search and verify full results return

**Expected Results**:
- ✅ Search finds tasks by title
- ✅ Search finds tasks by description content
- ✅ Search is case-insensitive
- ✅ Search combines with filters
- ✅ No results message displayed appropriately

---

## Test Suite 6: Mobile and Touch Interface

### Test 6.1: Mobile Responsive Design
**Objective**: Verify mobile interface works correctly

**Device Setup**: iPhone, Android phone, iPad, Android tablet

**Steps**:
1. Open kanban board on mobile device
2. Verify responsive layout:
   - Columns stack vertically or scroll horizontally
   - Task cards readable and touchable
   - Navigation menu accessible
3. Test touch interactions:
   - Tap to select task
   - Long press to show context menu
   - Swipe to scroll between columns
4. Create new task on mobile:
   - Use on-screen keyboard
   - Select assignee from dropdown
   - Set due date with date picker

**Expected Results**:
- ✅ Board displays properly on small screens
- ✅ Touch targets are adequately sized (44px minimum)
- ✅ Text remains readable without zooming
- ✅ Touch interactions feel responsive
- ✅ Form inputs work with mobile keyboards

### Test 6.2: Touch-Based Drag and Drop
**Objective**: Verify touch drag-and-drop on mobile devices

**Steps**:
1. **iPhone/Android**: Touch and hold task card
2. Observe haptic feedback (if device supports)
3. Drag task to different column:
   - Watch for visual feedback
   - Observe auto-scroll when reaching edge
4. Drop task in new position
5. Test multi-finger gestures:
   - Pinch to zoom (should be disabled on board)
   - Two-finger scroll for navigation
6. Test error conditions:
   - Drag to invalid location
   - Drag when WIP limit reached

**Expected Results**:
- ✅ Long press initiates drag mode
- ✅ Haptic feedback provided (if available)
- ✅ Visual feedback clear during drag
- ✅ Auto-scroll works at screen edges
- ✅ Drop zones highlighted appropriately
- ✅ Error feedback clear and helpful

### Test 6.3: Mobile Performance
**Objective**: Verify performance on mobile devices

**Steps**:
1. Load board with 100+ tasks on mobile
2. Measure load time (should be < 5 seconds)
3. Test smooth scrolling through columns
4. Perform multiple task moves rapidly
5. Test during poor network conditions:
   - Enable airplane mode temporarily
   - Try to move tasks
   - Re-enable network
   - Verify sync occurs

**Expected Results**:
- ✅ Board loads within 5 seconds on mobile
- ✅ Scrolling is smooth (60fps target)
- ✅ Touch operations feel responsive
- ✅ Offline mode handled gracefully
- ✅ Sync works when connection restored

---

## Test Suite 7: Performance and Load

### Test 7.1: Large Board Performance
**Objective**: Verify performance with large datasets

**Preparation**: Create board with 500+ tasks distributed across columns

**Steps**:
1. Open board and measure load time
2. Scroll through all columns smoothly
3. Apply various filters and measure response time
4. Perform search across all tasks
5. Move tasks between columns
6. Test real-time updates with multiple users

**Performance Targets**:
- ✅ Initial load: < 3 seconds
- ✅ Filter application: < 1 second
- ✅ Search results: < 1 second
- ✅ Task movement: < 500ms
- ✅ Real-time updates: < 200ms

### Test 7.2: Concurrent User Load
**Objective**: Verify system handles multiple simultaneous users

**Setup**: 10+ users accessing same board simultaneously

**Steps**:
1. All users join same board
2. Monitor presence list (should show all users)
3. Users perform various actions simultaneously:
   - Moving different tasks
   - Creating new tasks
   - Editing task details
4. Monitor for conflicts and resolution
5. Check database consistency after test

**Expected Results**:
- ✅ All users see consistent board state
- ✅ No data corruption or loss
- ✅ Conflict resolution works properly
- ✅ Performance remains acceptable
- ✅ System recovers gracefully from conflicts

---

## Test Suite 8: Error Handling and Edge Cases

### Test 8.1: Network Interruption Handling
**Objective**: Verify graceful handling of network issues

**Steps**:
1. Disconnect network while using board
2. Try to move tasks (should queue operations)
3. Observe offline indicator
4. Reconnect network
5. Verify queued operations execute
6. Test partial network failure (slow connection)

**Expected Results**:
- ✅ Offline state clearly indicated
- ✅ Operations queued locally
- ✅ Automatic reconnection attempts
- ✅ Sync occurs when connection restored
- ✅ No data loss during network issues

### Test 8.2: Browser Compatibility
**Objective**: Verify cross-browser functionality

**Test Matrix**:
- Chrome (latest, previous version)
- Firefox (latest, previous version)  
- Safari (latest, previous version)
- Edge (latest, previous version)

**Steps for each browser**:
1. Open kanban board
2. Verify all features work:
   - Drag and drop
   - Real-time updates
   - Modal dialogs
   - Form interactions
3. Test WebSocket connections
4. Verify visual appearance

**Expected Results**:
- ✅ Feature parity across browsers
- ✅ Consistent visual appearance
- ✅ WebSocket connections stable
- ✅ No JavaScript errors

### Test 8.3: Data Validation and Security
**Objective**: Verify proper validation and security measures

**Steps**:
1. Test input validation:
   - Extremely long task titles
   - Special characters in descriptions
   - Invalid date formats
   - SQL injection attempts
2. Test authorization:
   - Access board without permission
   - Try to modify others' tasks
   - Attempt admin operations as regular user
3. Test XSS protection:
   - Enter script tags in task content
   - Verify proper escaping

**Expected Results**:
- ✅ Input validation prevents invalid data
- ✅ Authorization properly enforced
- ✅ XSS attacks prevented
- ✅ Error messages helpful but not revealing

---

## Test Completion Checklist

### Functional Requirements ✓
- [ ] All user stories from feature-spec.md tested
- [ ] Kanban board CRUD operations work
- [ ] Task movement and positioning accurate
- [ ] Real-time collaboration functional
- [ ] WIP limits enforced correctly
- [ ] Swimlane organization works
- [ ] Filtering and search functional
- [ ] Mobile interface responsive and usable

### Performance Requirements ✓
- [ ] Board loads within 3 seconds (500+ tasks)
- [ ] Task movements complete within 500ms
- [ ] Real-time updates propagate within 200ms
- [ ] Mobile performance acceptable
- [ ] Concurrent user scenarios handled

### Quality Requirements ✓
- [ ] Cross-browser compatibility verified
- [ ] Mobile devices tested (iOS/Android)
- [ ] Error handling graceful
- [ ] Security measures effective
- [ ] Data integrity maintained
- [ ] Accessibility requirements met

### Documentation ✓
- [ ] All test cases executed
- [ ] Issues documented and tracked
- [ ] Performance metrics recorded
- [ ] Sign-off obtained from stakeholders

---

## Post-Testing Actions

### Issue Tracking
1. Document all discovered issues in project tracker
2. Classify by severity: Critical, High, Medium, Low
3. Assign owners and target fix dates
4. Retest after fixes implemented

### Performance Baseline
1. Record all performance measurements
2. Compare against targets in feature specification
3. Document any optimizations needed
4. Plan performance monitoring for production

### User Acceptance
1. Conduct user acceptance testing with product managers
2. Gather feedback on usability and workflow
3. Document any requested enhancements
4. Plan training materials if needed

---

**Manual Testing Status: COMPLETE ✅**

All manual testing scenarios defined and ready for execution. The guide covers comprehensive validation of enhanced kanban board functionality across web and mobile platforms with focus on real-time collaboration, performance, and user experience.