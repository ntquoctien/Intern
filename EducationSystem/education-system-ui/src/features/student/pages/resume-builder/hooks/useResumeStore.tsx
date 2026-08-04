import React, { createContext, useCallback, useContext, useEffect, useReducer } from 'react'
import type {
  AwardActivityItem,
  ApprovedInternship,
  CertificationItem,
  EligibleCourse,
  OptimizedBulletSection,
  OptimizedResumeResponseDto,
  OptimizedSkillGroup,
  PrepareResumePayloadRequestDto,
  ResumeContactInfo,
  ResumeBuilderAction,
  ResumeBuilderState,
  StudentProject,
  UiProjectOverride,
} from '../types'
import { createTemporaryProjectId } from '../utils/localIds'

const STORAGE_KEY = 'tdu.resume-builder.wizard.v1'

const MIN_ZOOM = 0.3
const MAX_ZOOM = 2

const initialState: ResumeBuilderState = {
  currentStep: 0,
  contextLoaded: false,
  targetRole: '',
  jobDescription: '',
  careerFocusTag: '',
  selectedSubjectIds: [],
  selectedInternshipIds: [],
  uiProjects: [],
  certifications: [],
  awardsAndActivities: [],
  optimizedCvResult: null,
  preparePayload: null,
  isLoadingContext: false,
  isOptimizingAi: false,
  zoomLevel: null,
  studentInfo: {
    fullName: '',
    studentCode: '',
    majorName: '',
    gpa: 0,
    academicYear: '',
  },
  contactInfo: {
    email: '',
    phone: '',
    address: '',
    github: '',
    linkedin: '',
  },
  eligibleCourses: [],
  projects: [],
  approvedInternships: [],
}

/** Khôi phục state từ sessionStorage để F5 không mất dữ liệu khi đang thao tác/demo. */
function loadPersistedState(): ResumeBuilderState {
  try {
    const raw = window.sessionStorage.getItem(STORAGE_KEY)
    if (!raw) return initialState
    const parsed = JSON.parse(raw) as Partial<ResumeBuilderState>
    const migratedCourses = (parsed.eligibleCourses ?? [])
      .map(course => ({
        ...course,
        recommendationSource: course.recommendationSource
          ?? (course.similarityScore !== undefined ? 'vector' as const : 'score-fallback' as const),
        matchedOutcomes: course.matchedOutcomes ?? [],
      }))
      .sort((a, b) =>
        (b.similarityScore ?? 0) - (a.similarityScore ?? 0) || b.score - a.score,
      )
      .slice(0, 10)
    const migratedCourseIds = new Set(migratedCourses.map(course => course.subjectId))
    const migratedProjects = (parsed.uiProjects ?? []).map((project, index) => {
      const safeProjectId = Number.isInteger(project.projectId) && project.projectId > 0 && project.projectId <= 2_147_483_647
        ? project.projectId
        : createTemporaryProjectId() - index
      return {
        ...project,
        projectId: safeProjectId,
      }
    })
    const migratedOptimizedCv = parsed.optimizedCvResult
      ? {
          ...parsed.optimizedCvResult,
          skills: Object.fromEntries(
            Object.entries(parsed.optimizedCvResult.skills).map(([group, skills]) => [
              group,
              skills.map(skill => {
                const legacy = skill as typeof skill & { description?: string }
                return {
                  ...skill,
                  keywords: Array.isArray(skill.keywords)
                    ? skill.keywords
                    : legacy.description?.trim()
                      ? [legacy.description.trim()]
                      : [],
                }
              }),
            ]),
          ) as OptimizedResumeResponseDto['skills'],
        }
      : null
    return {
      ...initialState,
      ...parsed,
      eligibleCourses: migratedCourses,
      selectedSubjectIds: (parsed.selectedSubjectIds ?? [])
        .filter(subjectId => migratedCourseIds.has(subjectId)),
      uiProjects: migratedProjects,
      optimizedCvResult: migratedOptimizedCv,
      // Trạng thái transient không khôi phục
      isLoadingContext: false,
      isOptimizingAi: false,
      zoomLevel: null,
      studentInfo: { ...initialState.studentInfo, ...(parsed.studentInfo ?? {}) },
      contactInfo: { ...initialState.contactInfo, ...(parsed.contactInfo ?? {}) },
    }
  } catch {
    return initialState
  }
}

function resumeBuilderReducer(state: ResumeBuilderState, action: ResumeBuilderAction): ResumeBuilderState {
  switch (action.type) {
    case 'SET_STEP':
      return { ...state, currentStep: action.payload }
    case 'SET_TARGET_JD':
      return {
        ...state,
        targetRole: action.payload.role,
        jobDescription: action.payload.jd,
        careerFocusTag: action.payload.careerFocusTag,
      }
    case 'SET_CONTACT_INFO':
      return { ...state, contactInfo: action.payload }
    case 'TOGGLE_SUBJECT': {
      const id = action.payload
      const selected = state.selectedSubjectIds.includes(id)
        ? state.selectedSubjectIds.filter(s => s !== id)
        : [...state.selectedSubjectIds, id]
      return { ...state, selectedSubjectIds: selected }
    }
    case 'TOGGLE_INTERNSHIP': {
      const id = action.payload
      const selected = state.selectedInternshipIds.includes(id)
        ? state.selectedInternshipIds.filter(s => s !== id)
        : [...state.selectedInternshipIds, id]
      return { ...state, selectedInternshipIds: selected }
    }
    case 'TOGGLE_PROJECT_SELECTION': {
      const projectId = action.payload
      const projects = state.uiProjects.map(p =>
        p.projectId === projectId
          ? { ...p, isSelectedForCv: !p.isSelectedForCv }
          : p,
      )
      return { ...state, uiProjects: projects }
    }
    case 'TOGGLE_CERTIFICATION_SELECTION': {
      const certificationId = action.payload
      return {
        ...state,
        certifications: state.certifications.map(item =>
          item.id === certificationId ? { ...item, isSelectedForCv: !item.isSelectedForCv } : item,
        ),
      }
    }
    case 'TOGGLE_AWARD_SELECTION': {
      const awardId = action.payload
      return {
        ...state,
        awardsAndActivities: state.awardsAndActivities.map(item =>
          item.id === awardId ? { ...item, isSelectedForCv: !item.isSelectedForCv } : item,
        ),
      }
    }
    case 'UPDATE_PROJECT': {
      const projects = [...state.uiProjects]
      projects[action.payload.index] = action.payload.project
      return { ...state, uiProjects: projects }
    }
    case 'ADD_PERSONAL_PROJECT': {
      const newProject = { ...action.payload, source: 'personal' as const }
      return { ...state, uiProjects: [...state.uiProjects, newProject] }
    }
    case 'DELETE_PERSONAL_PROJECT': {
      return {
        ...state,
        uiProjects: state.uiProjects.filter(p => p.projectId !== action.payload),
      }
    }
    case 'ADD_CERTIFICATION':
      return { ...state, certifications: [...state.certifications, action.payload] }
    case 'UPDATE_CERTIFICATION': {
      const items = [...state.certifications]
      items[action.payload.index] = action.payload.certification
      return { ...state, certifications: items }
    }
    case 'DELETE_CERTIFICATION':
      return {
        ...state,
        certifications: state.certifications.filter(item => item.id !== action.payload),
      }
    case 'ADD_AWARD':
      return { ...state, awardsAndActivities: [...state.awardsAndActivities, action.payload] }
    case 'UPDATE_AWARD': {
      const items = [...state.awardsAndActivities]
      items[action.payload.index] = action.payload.award
      return { ...state, awardsAndActivities: items }
    }
    case 'DELETE_AWARD':
      return {
        ...state,
        awardsAndActivities: state.awardsAndActivities.filter(item => item.id !== action.payload),
      }
    case 'LINK_PROJECT_COURSES': {
      const projects = state.uiProjects.map(p =>
        p.projectId === action.payload.projectId
          ? { ...p, linkedCourseIds: action.payload.courseIds }
          : p,
      )
      return { ...state, uiProjects: projects }
    }
    case 'SET_LOADING_CONTEXT':
      return { ...state, isLoadingContext: action.payload }
    case 'SET_OPTIMIZING':
      return { ...state, isOptimizingAi: action.payload }
    case 'SET_ZOOM':
      return {
        ...state,
        zoomLevel: action.payload === null
          ? null
          : Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, action.payload)),
      }
    case 'SET_CONTEXT_DATA':
      return {
        ...state,
        contextLoaded: true,
        selectedSubjectIds: action.payload.courses.slice(0, 3).map(course => course.subjectId),
        studentInfo: action.payload.studentInfo,
        eligibleCourses: action.payload.courses,
        projects: action.payload.projects,
        approvedInternships: action.payload.internships,
        uiProjects: action.payload.projects.map(p => ({
          projectId: p.projectId,
          projectName: p.projectName,
          techStack: p.techStack,
          sourceCodeUrl: p.sourceCodeUrl,
          myRole: p.myRole ?? '',
          myContributions: p.myContributions ?? '',
          teamSize: p.teamSize,
        })),
        optimizedCvResult: null,
        preparePayload: null,
      }
    case 'PREPARE_PAYLOAD':
      return { ...state, preparePayload: action.payload }
    case 'SET_OPTIMIZED_RESULT':
      return { ...state, optimizedCvResult: action.payload }
    case 'UPDATE_OPTIMIZED_SUMMARY': {
      if (!state.optimizedCvResult) return state
      return {
        ...state,
        optimizedCvResult: { ...state.optimizedCvResult, professionalSummary: action.payload },
      }
    }
    case 'UPDATE_OPTIMIZED_SKILL': {
      const cv = state.optimizedCvResult
      if (!cv) return state
      const { group, index, keywords } = action.payload
      return {
        ...state,
        optimizedCvResult: {
          ...cv,
          skills: {
            ...cv.skills,
            [group]: cv.skills[group].map((skill, i) => (i === index ? { ...skill, keywords } : skill)),
          },
        },
      }
    }
    case 'UPDATE_OPTIMIZED_BULLET': {
      const cv = state.optimizedCvResult
      if (!cv) return state
      const { section, index, bulletIndex, value } = action.payload
      if (section === 'projects') {
        return {
          ...state,
          optimizedCvResult: {
            ...cv,
            projects: cv.projects.map((project, i) =>
              i === index
                ? { ...project, actionBulletPoints: project.actionBulletPoints.map((b, j) => (j === bulletIndex ? value : b)) }
                : project),
          },
        }
      }
      return {
        ...state,
        optimizedCvResult: {
          ...cv,
          internships: cv.internships.map((internship, i) =>
            i === index
              ? { ...internship, actionBulletPoints: internship.actionBulletPoints.map((b, j) => (j === bulletIndex ? value : b)) }
              : internship),
        },
      }
    }
    case 'RESET':
      return initialState
    default:
      return state
  }
}

export interface ResumeStoreContextType {
  state: ResumeBuilderState
  dispatch: React.Dispatch<ResumeBuilderAction>
  setStep: (step: number) => void
  setTargetJd: (role: string, jd: string, careerFocusTag?: string) => void
  setContactInfo: (contactInfo: ResumeContactInfo) => void
  toggleSubjectSelection: (subjectId: string) => void
  toggleInternshipSelection: (internshipId: number) => void
  toggleProjectSelection: (projectId: number) => void
  toggleCertificationSelection: (certificationId: number) => void
  toggleAwardSelection: (awardId: number) => void
  updateUiProject: (index: number, project: UiProjectOverride) => void
  addPersonalProject: (project: UiProjectOverride) => void
  deletePersonalProject: (projectId: number) => void
  addCertification: (certification: CertificationItem) => void
  updateCertification: (index: number, certification: CertificationItem) => void
  deleteCertification: (certificationId: number) => void
  addAward: (award: AwardActivityItem) => void
  updateAward: (index: number, award: AwardActivityItem) => void
  deleteAward: (awardId: number) => void
  linkProjectCourses: (projectId: number, courseIds: string[]) => void
  setLoadingContext: (loading: boolean) => void
  setOptimizing: (optimizing: boolean) => void
  /** null = trở về chế độ auto-fit */
  setZoomLevel: (zoom: number | null) => void
  setContextData: (data: {
    studentInfo: ResumeBuilderState['studentInfo']
    courses: EligibleCourse[]
    projects: StudentProject[]
    internships: ApprovedInternship[]
  }) => void
  preparePayload: (payload: PrepareResumePayloadRequestDto) => void
  setOptimizedResult: (result: OptimizedResumeResponseDto) => void
  updateOptimizedSummary: (summary: string) => void
  updateOptimizedSkill: (group: OptimizedSkillGroup, index: number, keywords: string[]) => void
  updateOptimizedBullet: (section: OptimizedBulletSection, index: number, bulletIndex: number, value: string) => void
  reset: () => void
}

const ResumeStoreContext = createContext<ResumeStoreContextType | null>(null)

export const ResumeStoreProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(resumeBuilderReducer, undefined, loadPersistedState)

  // Persist vào sessionStorage sau mỗi thay đổi (bỏ qua trạng thái transient).
  useEffect(() => {
    try {
      const persistable: ResumeBuilderState = {
        ...state,
        isLoadingContext: false,
        isOptimizingAi: false,
        zoomLevel: null,
      }
      window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(persistable))
    } catch {
      // Bỏ qua lỗi quota / privacy mode
    }
  }, [state])

  const setStep = useCallback((step: number) => dispatch({ type: 'SET_STEP', payload: step }), [])
  const setTargetJd = useCallback(
    (role: string, jd: string, careerFocusTag = '') =>
      dispatch({ type: 'SET_TARGET_JD', payload: { role, jd, careerFocusTag } }),
    [],
  )
  const setContactInfo = useCallback(
    (contactInfo: ResumeContactInfo) => dispatch({ type: 'SET_CONTACT_INFO', payload: contactInfo }),
    [],
  )
  const toggleSubjectSelection = useCallback((subjectId: string) => dispatch({ type: 'TOGGLE_SUBJECT', payload: subjectId }), [])
  const toggleInternshipSelection = useCallback((internshipId: number) => dispatch({ type: 'TOGGLE_INTERNSHIP', payload: internshipId }), [])
  const toggleProjectSelection = useCallback((projectId: number) => dispatch({ type: 'TOGGLE_PROJECT_SELECTION', payload: projectId }), [])
  const toggleCertificationSelection = useCallback((certificationId: number) => dispatch({ type: 'TOGGLE_CERTIFICATION_SELECTION', payload: certificationId }), [])
  const toggleAwardSelection = useCallback((awardId: number) => dispatch({ type: 'TOGGLE_AWARD_SELECTION', payload: awardId }), [])
  const updateUiProject = useCallback((index: number, project: UiProjectOverride) => dispatch({ type: 'UPDATE_PROJECT', payload: { index, project } }), [])
  const addPersonalProject = useCallback((project: UiProjectOverride) => dispatch({ type: 'ADD_PERSONAL_PROJECT', payload: project }), [])
  const deletePersonalProject = useCallback((projectId: number) => dispatch({ type: 'DELETE_PERSONAL_PROJECT', payload: projectId }), [])
  const addCertification = useCallback((certification: CertificationItem) => dispatch({ type: 'ADD_CERTIFICATION', payload: certification }), [])
  const updateCertification = useCallback((index: number, certification: CertificationItem) => dispatch({ type: 'UPDATE_CERTIFICATION', payload: { index, certification } }), [])
  const deleteCertification = useCallback((certificationId: number) => dispatch({ type: 'DELETE_CERTIFICATION', payload: certificationId }), [])
  const addAward = useCallback((award: AwardActivityItem) => dispatch({ type: 'ADD_AWARD', payload: award }), [])
  const updateAward = useCallback((index: number, award: AwardActivityItem) => dispatch({ type: 'UPDATE_AWARD', payload: { index, award } }), [])
  const deleteAward = useCallback((awardId: number) => dispatch({ type: 'DELETE_AWARD', payload: awardId }), [])
  const linkProjectCourses = useCallback((projectId: number, courseIds: string[]) => dispatch({ type: 'LINK_PROJECT_COURSES', payload: { projectId, courseIds } }), [])
  const setLoadingContext = useCallback((loading: boolean) => dispatch({ type: 'SET_LOADING_CONTEXT', payload: loading }), [])
  const setOptimizing = useCallback((optimizing: boolean) => dispatch({ type: 'SET_OPTIMIZING', payload: optimizing }), [])
  const setZoomLevel = useCallback((zoom: number | null) => dispatch({ type: 'SET_ZOOM', payload: zoom }), [])
  const setContextData = useCallback((data: {
    studentInfo: ResumeBuilderState['studentInfo']
    courses: EligibleCourse[]
    projects: StudentProject[]
    internships: ApprovedInternship[]
  }) => dispatch({ type: 'SET_CONTEXT_DATA', payload: data }), [])
  const preparePayloadCallback = useCallback((payload: PrepareResumePayloadRequestDto) => dispatch({ type: 'PREPARE_PAYLOAD', payload }), [])
  const setOptimizedResult = useCallback((result: OptimizedResumeResponseDto) => dispatch({ type: 'SET_OPTIMIZED_RESULT', payload: result }), [])
  const updateOptimizedSummary = useCallback((summary: string) => dispatch({ type: 'UPDATE_OPTIMIZED_SUMMARY', payload: summary }), [])
  const updateOptimizedSkill = useCallback(
    (group: OptimizedSkillGroup, index: number, keywords: string[]) =>
      dispatch({ type: 'UPDATE_OPTIMIZED_SKILL', payload: { group, index, keywords } }),
    [],
  )
  const updateOptimizedBullet = useCallback(
    (section: OptimizedBulletSection, index: number, bulletIndex: number, value: string) =>
      dispatch({ type: 'UPDATE_OPTIMIZED_BULLET', payload: { section, index, bulletIndex, value } }),
    [],
  )
  const reset = useCallback(() => {
    try {
      window.sessionStorage.removeItem(STORAGE_KEY)
    } catch {
      // Bỏ qua
    }
    dispatch({ type: 'RESET' })
  }, [])

  const value = React.useMemo<ResumeStoreContextType>(() => ({
    state,
    dispatch,
    setStep,
    setTargetJd,
    setContactInfo,
    toggleSubjectSelection,
    toggleInternshipSelection,
    toggleProjectSelection,
    toggleCertificationSelection,
    toggleAwardSelection,
    updateUiProject,
    addPersonalProject,
    deletePersonalProject,
    addCertification,
    updateCertification,
    deleteCertification,
    addAward,
    updateAward,
    deleteAward,
    linkProjectCourses,
    setLoadingContext,
    setOptimizing,
    setZoomLevel,
    setContextData,
    preparePayload: preparePayloadCallback,
    setOptimizedResult,
    updateOptimizedSummary,
    updateOptimizedSkill,
    updateOptimizedBullet,
    reset,
  }), [state, setStep, setTargetJd, setContactInfo, toggleSubjectSelection, toggleInternshipSelection, toggleProjectSelection, toggleCertificationSelection, toggleAwardSelection, updateUiProject, addPersonalProject, deletePersonalProject, addCertification, updateCertification, deleteCertification, addAward, updateAward, deleteAward, linkProjectCourses, setLoadingContext, setOptimizing, setZoomLevel, setContextData, preparePayloadCallback, setOptimizedResult, updateOptimizedSummary, updateOptimizedSkill, updateOptimizedBullet, reset])

  return (
    <ResumeStoreContext.Provider value={value}>
      {children}
    </ResumeStoreContext.Provider>
  )
}

export function useResumeStore(): ResumeStoreContextType {
  const context = useContext(ResumeStoreContext)
  if (!context) {
    throw new Error('useResumeStore must be used within a ResumeStoreProvider')
  }
  return context
}
