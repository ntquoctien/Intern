# Education System UI - Comprehensive Refactoring Guide

## 📋 Overview

This guide documents the complete UI/UX refactoring of the Education System frontend, transforming it from a raw database viewer into a polished, user-friendly interface.

---

## 🎯 Phase 1: Helper Functions & Data Formatters

### Created Files

#### 1. `src/shared/utils/enumMappers.ts`
Maps raw numeric/string codes to business-friendly labels with color coding.

**Key Mappings:**
```typescript
ROLE_MAP: { 99: Admin, 50: Teacher, 1: Student }
STUDY_STATUS_MAP: { 0: Chưa bắt đầu, 1: Đã hoàn thành, 2: Đang học, ... }
SCHEDULE_TYPE_MAP: { 0: Lý thuyết, 1: Thực hành, 2: Seminar, ... }
GENDER_MAP: { 0: Nam, 1: Nữ, 2: Khác }
```

**Usage:**
```typescript
import { ROLE_MAP, getEnumLabel } from '@/shared/utils/enumMappers'

const roleLabel = getEnumLabel(99, ROLE_MAP) // Returns: "Quản trị viên"
```

#### 2. `src/shared/utils/dataFormatters.ts`
Comprehensive formatters for all data types (dates, strings, numbers, enums).

**Categories:**

| Category | Functions |
|----------|-----------|
| **Dates** | `formatDateOnly()`, `formatDateTime()`, `formatTime()` |
| **Strings** | `formatSubjectCode()`, `cleanPrefix()`, `truncateString()` |
| **Enums** | `formatRole()`, `formatStudyStatus()`, `formatScheduleType()`, `formatGender()` |
| **Numbers** | `formatNumber()`, `formatPercent()`, `formatDecimal()` |
| **Units** | `formatHours()`, `formatCredits()` |
| **Empty Values** | `formatEmptyValue()` |
| **Composite** | `formatStudentReference()`, `formatSubjectReference()` |

**Usage Examples:**
```typescript
import {
  formatDateOnly,
  formatRole,
  getRoleColor,
  formatHours,
} from '@/shared/utils/dataFormatters'

// Dates
formatDateOnly('2000-01-01 00:00:00') // Returns: "01/01/2000"

// Enums with color
const label = formatRole(99) // "Quản trị viên"
const color = getRoleColor(99) // "red"

// Units
formatHours(45) // "45 giờ"
formatCredits(3) // "3 tín chỉ"
```

---

## 🎯 Phase 2: Updated Table Renderers

### File: `src/shared/components/tableRenderers.tsx`

New renderer functions for cleaner table displays:

```typescript
// Status/Enum Renderers with Badges
roleTag(role: number) → <Tag color="red">Quản trị viên</Tag>
statusTag(status: number) → <Tag color="green">Đang học</Tag>
genderTag(gender: number) → <span>Nam</span>
scheduleTypeTag(type: number) → <Tag color="blue">Lý thuyết</Tag>
attendanceTag(status: number) → <Tag color="green">Có mặt</Tag>
examResultTag(result: number) → <Tag color="green">Đạt</Tag>

// Unit Renderers
hoursRenderer(hours: number) → "45 giờ"
creditsRenderer(credits: number) → "3 tín chỉ"

// Date Renderers
dateOnly(dateStr: string) → "01/01/2000"
dateTime(dateStr: string) → "01/01/2000 14:30"

// String Cleaners
cleanStudentReference(value: string) → "SV000234" (removes "Student: " prefix)
cleanSubjectReference(value: string) → "KT-101" (removes "Subject: " prefix)

// Empty Value Handler
formatEmptyValue(value: unknown) → "-" or formatted value
```

---

## 🎯 Phase 3: Updated Column Definitions & Page Descriptions

### Academic Pages (`src/features/academic/pages.tsx`)

#### Students Page
```typescript
// Before
{ title: 'Nickname', dataIndex: 'nickname' }
{ title: 'Study Status', dataIndex: 'studyStatus' }
{ title: 'Gender', dataIndex: 'gender' }

// After
{ title: 'Mã học viên', dataIndex: 'nickname' }
{ title: 'Trạng thái', dataIndex: 'studyStatus', render: statusTag }
{ title: 'Giới tính', dataIndex: 'gender', render: genderTag }
```

**Description Update:**
```typescript
// Before: "Read-only student records from AcademicService"
// After: "Quản lý hồ sơ và thông tin học viên"
```

#### Subjects Page
```typescript
// Column Updates
{ title: 'Mã môn học', dataIndex: 'subjectCode' }
{ title: 'Tín chỉ', dataIndex: 'creditPoint', render: creditsRenderer }
{ title: 'Giờ học', dataIndex: 'totalHours', render: hoursRenderer }
```

#### Subject Schedules Page
```typescript
// New renderers
{ title: 'Loại', dataIndex: 'scheduleType', render: scheduleTypeTag }
// Shows "Lý thuyết", "Thực hành" etc instead of 0, 1
```

#### Attendance Page
```typescript
// Cleaned student reference
{ title: 'Học viên', dataIndex: 'studentId', render: cleanStudentReference }
// Shows "SV000234" instead of "Student: SV000234"
```

---

## 🔧 Implementation Checklist

### For Each Page/Table:

- [ ] Update page title to Vietnamese business language
- [ ] Replace description with user-friendly text (remove "Service" references)
- [ ] Update column titles to Vietnamese
- [ ] Add appropriate render functions to columns
- [ ] Update search placeholder to Vietnamese
- [ ] Update search help text to user-friendly language
- [ ] Test with actual data to ensure formatting works

### Example: Adding New Formatter

```typescript
// 1. Add to enumMappers.ts if needed
export const NEW_STATUS_MAP = {
  0: { label: 'Status 0', color: 'blue' },
  1: { label: 'Status 1', color: 'green' },
}

// 2. Add to dataFormatters.ts
export function formatNewStatus(status?: number | null): string {
  return getEnumLabel(status, NEW_STATUS_MAP, '-')
}

export function getNewStatusColor(status?: number | null): string {
  return getEnumColor(status, NEW_STATUS_MAP, 'default')
}

// 3. Add to tableRenderers.tsx
export function newStatusTag(status?: number) {
  const label = formatNewStatus(status)
  const color = getNewStatusColor(status)
  return label === '-' ? '-' : <Tag color={color}>{label}</Tag>
}

// 4. Use in page column definition
{ title: 'Status', dataIndex: 'status', render: newStatusTag }
```

---

## 📊 Sample Implementation: Users Page

```typescript
// src/features/identity/pages.tsx
import { roleTag, dateOnly } from '@/shared/components/tableRenderers'

const userColumns: ColumnsType<User> = [
  { title: 'Tên tài khoản', dataIndex: 'userName', sorter: true },
  { title: 'Họ và tên', dataIndex: 'fullName', sorter: true },
  { title: 'Email', dataIndex: 'email', sorter: true },
  { title: 'Chức vụ', dataIndex: 'role', sorter: true, render: roleTag },
  { title: 'Ngày sinh', dataIndex: 'dateOfBirth', sorter: true, render: dateOnly },
  { title: 'Đang hoạt động', dataIndex: 'isActive', render: boolTag },
]

export function UsersPage() {
  return (
    <DataTablePage<User>
      title="Danh sách tài khoản"
      description="Quản lý tài khoản người dùng trong hệ thống"
      service="identity"
      resourcePath="users"
      columns={userColumns}
      searchPlaceholder="Tìm kiếm theo tên tài khoản hoặc email"
      searchHelp="Nhập tên tài khoản hoặc email"
    />
  )
}
```

---

## 🎨 Color Scheme for Badges

| Status | Color | Usage |
|--------|-------|-------|
| Primary/Success | `green` | Completed, Active, Present |
| Warning | `orange` | In Progress, Late, Warning |
| Danger | `red` | Error, Failed, Absent, Admin |
| Info | `blue` | Neutral, Academic Year, Passed |
| Default | `default` | Unknown, N/A |

---

## 🚀 CSS Fixes for Layout Issues

### Column Header Truncation Fix

**Problem:** Column "D" header in Subject Teachings showing truncated text

**Solution:**
Add to global CSS or Ant Design theme config:

```css
/* src/index.css or theme file */
.ant-table-thead > tr > th {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  min-width: 120px; /* Adjust based on content */
}

.ant-table-cell {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
```

Or configure column width explicitly:

```typescript
{
  title: 'Loại',
  dataIndex: 'scheduleType',
  width: 150, // Explicit width
  render: scheduleTypeTag
}
```

---

## 📝 Translation Reference

### Common Translations

| English | Vietnamese |
|---------|-----------|
| Students | Danh sách học viên |
| Subjects | Quản lý môn học |
| Subject Teaching | Lớp học phần |
| Subject Students | Danh sách học viên theo lớp |
| Subject Schedules | Lịch học chi tiết |
| Attendance | Điểm danh |
| Exam Results | Kết quả thi |
| Questions | Ngân hàng câu hỏi |
| Users | Danh sách tài khoản |
| User Directory | Danh sách tài khoản hệ thống |
| Read-only records | Thông tin được quản lý |
| Admin | Quản trị viên |
| Teacher | Giáo viên |
| Student | Học viên |

---

## ✅ Testing Checklist

- [ ] Date formatting shows correct format (DD/MM/YYYY)
- [ ] Enum values display as labels, not numbers
- [ ] Badges show correct colors
- [ ] Empty/null values display as "-"
- [ ] Prefixes are removed correctly
- [ ] Units (giờ, tín chỉ) display correctly
- [ ] Vietnamese text renders properly
- [ ] Sorting works on formatted columns
- [ ] Search works with user-friendly help text
- [ ] Responsive design maintained

---

## 🔄 Future Enhancements

1. **Localization**: Create i18n configuration for multi-language support
2. **Custom Formatters**: Add domain-specific formatters (GPA, rank, etc.)
3. **Export**: Format exported data consistently
4. **Filters**: Create filter components for formatted enum values
5. **Validation**: Add validation formatters for form inputs

---

## 📚 File Structure Summary

```
src/shared/utils/
├── enumMappers.ts          ✅ Created - Enum mappings
└── dataFormatters.ts       ✅ Created - Data formatting functions

src/shared/components/
└── tableRenderers.tsx      ✅ Updated - Renderer functions

src/features/
├── academic/
│   └── pages.tsx           ✅ Updated - Academic pages
├── exam/
│   └── pages.tsx           ⏳ Pending - Exam pages
├── identity/
│   └── pages.tsx           ⏳ Pending - Identity pages
└── (other modules)         ⏳ Pending - Other modules
```

---

## 🎓 Quick Reference

### To use a formatter in a column:

```typescript
import { formatRole, roleTag } from '@/shared/components/tableRenderers'

// For text display:
{ title: 'Role', dataIndex: 'role', render: (value) => formatRole(value) }

// For badge display:
{ title: 'Role', dataIndex: 'role', render: roleTag }
```

### To handle empty values:

```typescript
import { formatEmptyValue } from '@/shared/utils/dataFormatters'

// All these return "-" if value is null/undefined/empty
formatEmptyValue(null)
formatEmptyValue(undefined)
formatEmptyValue('')
```

### To format dates:

```typescript
import { formatDateOnly, formatDateTime } from '@/shared/utils/dataFormatters'

// Date only (for birthdate)
formatDateOnly('2000-01-01') // "01/01/2000"

// With time
formatDateTime('2025-09-03 14:30:00') // "03/09/2025 14:30"
```

---

## 🏁 Completion Status

- ✅ **Phase 1**: Helper functions created
- ✅ **Phase 2**: Table renderers updated
- ✅ **Phase 3**: Academic pages updated
- ⏳ **Remaining**: Exam & Identity pages (follow same pattern)
- ⏳ **Optional**: Localization, advanced formatters

---

**Last Updated**: July 10, 2026
**Status**: In Progress - Ready for module-by-module implementation
