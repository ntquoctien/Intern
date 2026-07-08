import { Navigate, createBrowserRouter } from 'react-router-dom'
import { MainLayout } from '../layouts/MainLayout'
import {
  AttendancesPage,
  StudentsPage,
  SubjectSchedulesPage,
  SubjectStudentsPage,
  SubjectTeachingsPage,
  SubjectsPage,
} from '../features/academic/pages'
import { ExamResultsPage, QuestionsPage } from '../features/exam/pages'
import { UsersPage } from '../features/identity/pages'
import { FormRequestsPage } from '../features/communication/pages'

export const router = createBrowserRouter([
  {
    element: <MainLayout />,
    children: [
      { index: true, element: <Navigate to="/academic/students" replace /> },
      { path: '/academic/students', element: <StudentsPage /> },
      { path: '/academic/subjects', element: <SubjectsPage /> },
      { path: '/academic/subject-teachings', element: <SubjectTeachingsPage /> },
      { path: '/academic/subject-students', element: <SubjectStudentsPage /> },
      { path: '/academic/subject-schedules', element: <SubjectSchedulesPage /> },
      { path: '/academic/attendances', element: <AttendancesPage /> },
      { path: '/exam/exam-results', element: <ExamResultsPage /> },
      { path: '/exam/questions', element: <QuestionsPage /> },
      { path: '/identity/users', element: <UsersPage /> },
      { path: '/communication/form-requests', element: <FormRequestsPage /> },
      { path: '*', element: <Navigate to="/academic/students" replace /> },
    ],
  },
])
