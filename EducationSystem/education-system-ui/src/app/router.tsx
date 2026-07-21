import { Navigate, createBrowserRouter, useLocation } from 'react-router-dom'
import { MainLayout } from '../layouts/MainLayout'
import {
  SubjectStudentsPage,
  SubjectsPage,
} from '../features/academic/pages'
import { UsersPage } from '../features/identity/pages'
import { FormRequestsPage } from '../features/communication/pages'
import { StudentLoginPage } from '../features/student/StudentLoginPage'
import { StudentProtectedLayout } from '../features/student/StudentLayout'
import { StudentAttendancePage, StudentDashboardPage, StudentExamResultsPage, StudentFormRequestsPage, StudentProfilePage, StudentProgramPage, StudentSchedulePage, StudentSubjectsPage } from '../features/student/StudentPages'
import { ManagementAttendancePage, ManagementClassesPage, ManagementDashboardPage, ManagementPlansPage, ManagementQuestionSuitesPage, ManagementResultsPage, ManagementSchedulePage, ManagementStudentsPage, ManagementTeachersPage } from '../features/management/ManagementPages'

export function LegacyRedirect({ to }: { to: string }) {
  const location = useLocation()
  return <Navigate to={`${to}${location.search}`} replace />
}

export const router = createBrowserRouter([
  { path: '/student/login', element: <StudentLoginPage /> },
  {
    element: <StudentProtectedLayout />,
    children: [
      { path: '/student', element: <Navigate to="/student/dashboard" replace /> },
      { path: '/student/dashboard', element: <StudentDashboardPage /> },
      { path: '/student/profile', element: <StudentProfilePage /> },
      { path: '/student/program', element: <StudentProgramPage /> },
      { path: '/student/subjects', element: <StudentSubjectsPage /> },
      { path: '/student/schedule', element: <StudentSchedulePage /> },
      { path: '/student/exam-results', element: <StudentExamResultsPage /> },
      { path: '/student/attendance', element: <StudentAttendancePage /> },
      { path: '/student/form-requests', element: <StudentFormRequestsPage /> },
    ],
  },
  {
    element: <MainLayout />,
    children: [
      { index: true, element: <Navigate to="/management/overview" replace /> },
      { path: '/management/overview', element: <ManagementDashboardPage /> },
      { path: '/management/education/plans', element: <ManagementPlansPage /> },
      { path: '/management/education/subjects', element: <SubjectsPage /> },
      { path: '/management/people/students', element: <ManagementStudentsPage /> },
      { path: '/management/people/teachers', element: <ManagementTeachersPage /> },
      { path: '/management/people/users', element: <UsersPage /> },
      { path: '/management/teaching/classes', element: <ManagementClassesPage /> },
      { path: '/management/teaching/assignments', element: <SubjectStudentsPage /> },
      { path: '/management/teaching/schedule', element: <ManagementSchedulePage /> },
      { path: '/management/teaching/attendance', element: <ManagementAttendancePage /> },
      { path: '/management/assessment/results', element: <ManagementResultsPage /> },
      { path: '/management/assessment/question-suites', element: <ManagementQuestionSuitesPage /> },
      { path: '/management/forms/requests', element: <FormRequestsPage /> },
      { path: '/academic/students', element: <LegacyRedirect to="/management/people/students" /> },
      { path: '/academic/subjects', element: <LegacyRedirect to="/management/education/subjects" /> },
      { path: '/academic/subject-teachings', element: <LegacyRedirect to="/management/teaching/classes" /> },
      { path: '/academic/subject-students', element: <LegacyRedirect to="/management/teaching/assignments" /> },
      { path: '/academic/subject-schedules', element: <LegacyRedirect to="/management/teaching/schedule" /> },
      { path: '/academic/attendances', element: <LegacyRedirect to="/management/teaching/attendance" /> },
      { path: '/exam/exam-results', element: <LegacyRedirect to="/management/assessment/results" /> },
      { path: '/exam/questions', element: <LegacyRedirect to="/management/assessment/question-suites" /> },
      { path: '/identity/users', element: <LegacyRedirect to="/management/people/users" /> },
      { path: '/communication/form-requests', element: <LegacyRedirect to="/management/forms/requests" /> },
      { path: '*', element: <Navigate to="/management/overview" replace /> },
    ],
  },
])
