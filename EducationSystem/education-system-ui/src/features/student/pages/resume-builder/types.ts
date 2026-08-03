// Types for Resume Builder Wizard

export type CloLevel = 'E' | 'R' | 'D'

export type EligibleCourse = {
  subjectId: string
  subjectCode: string
  subjectName: string
  score: number
  similarityScore?: number
  cloLevel?: CloLevel
  recommendationSource: 'vector' | 'score-fallback'
  matchedOutcomes: Array<{
    code: string
    description: string
    similarityScore: number
    cloLevel?: CloLevel
  }>
  courseOutcomes: Array<{ name: string; description?: string | null; cloLevel?: CloLevel }>
}

export type StudentProject = {
  projectId: number
  projectName: string
  techStack: string
  sourceCodeUrl?: string | null
  teamSize: number
  myRole?: string | null
  myContributions?: string | null
}

export type ApprovedInternship = {
  internshipId: number
  companyName: string
  position: string
  startDate: string
  endDate?: string | null
  taskDescription?: string | null
}

export type ResumeContextResponse = {
  student: { fullName: string; majorName: string }
  gpa?: number | null
  eligibleCourses: EligibleCourse[]
  projects: StudentProject[]
  approvedInternships: ApprovedInternship[]
}

export type ProjectSource = 'portfolio' | 'personal'

export type CertificationItem = {
  id: number
  name: string
  issuer: string
  issueDate?: string | null
  expirationDate?: string | null
  credentialUrl?: string | null
  isSelectedForCv: boolean
}

export type AwardActivityItem = {
  id: number
  title: string
  organization?: string | null
  achievedDate?: string | null
  description?: string | null
  isSelectedForCv: boolean
}

export type UiProjectOverride = {
  projectId: number
  projectName: string
  techStack: string
  sourceCodeUrl?: string | null
  myRole: string
  myContributions: string
  teamSize: number
  source?: ProjectSource
  linkedCourseIds?: string[]
  isSelectedForCv?: boolean
  mappedCourseId?: string
  mappedCourseCode?: string
  mappedCourseName?: string
}

export type OptimizedSkill = {
  skillName: string
  proficiency: 'Thành thạo' | 'Khá tốt' | 'Nền tảng'
  description: string
}

export type OptimizedResumeResponseDto = {
  header: {
    fullName: string
    studentCode: string
    majorName: string
    gpa?: number | null
    targetRole: string
  }
  education: {
    institutionName: string
    majorName: string
    degreeName: string
    gpa?: number | null
    durationText: string
  }
  professionalSummary: string
  skills: {
    knowledgeDomain: OptimizedSkill[]
    functionalSkills: OptimizedSkill[]
    interpersonalSkills: OptimizedSkill[]
  }
  projects: Array<{
    projectId: number
    projectName: string
    techStack: string
    myRole: string
    actionBulletPoints: string[]
  }>
  internships: Array<{
    internshipId: number
    companyName: string
    position: string
    durationText: string
    actionBulletPoints: string[]
  }>
  certifications: string[]
  awardsAndActivities: string[]
  qualityMetrics: {
    jobAlignmentScore: number
    contentPreservationScore: number
    hasHallucinationWarning: boolean
  }
}

export type OptimizedSkillGroup = 'knowledgeDomain' | 'functionalSkills' | 'interpersonalSkills'

export type OptimizedBulletSection = 'projects' | 'internships'

export type PrepareResumePayloadRequestDto = {
  studentId: string
  targetRole: string
  jobDescription: string
  careerFocusTag?: string | null
  selectedSubjectIds: string[]
  selectedInternshipIds: number[]
  uiProjects: UiProjectOverride[]
  certifications: string[]
  awardsAndActivities: string[]
  currentSummaryDraft?: string | null
}

export type ResumeContactInfo = {
  email: string
  phone: string
  address: string
  github: string
  linkedin: string
}

export type ResumeBuilderState = {
  currentStep: number
  /** true sau khi Bước 1 load context thành công — dùng để gating điều hướng wizard */
  contextLoaded: boolean
  targetRole: string
  jobDescription: string
  careerFocusTag: string
  selectedSubjectIds: string[]
  selectedInternshipIds: number[]
  uiProjects: UiProjectOverride[]
  certifications: CertificationItem[]
  awardsAndActivities: AwardActivityItem[]
  optimizedCvResult: OptimizedResumeResponseDto | null
  preparePayload: PrepareResumePayloadRequestDto | null
  isLoadingContext: boolean
  isOptimizingAi: boolean
  /** null = auto-fit theo độ rộng panel; number = zoom thủ công (0.3 – 2) */
  zoomLevel: number | null
  // Data from API
  studentInfo: {
    fullName: string
    studentCode: string
    majorName: string
    gpa: number
    academicYear: string
  }
  contactInfo: ResumeContactInfo
  eligibleCourses: EligibleCourse[]
  projects: StudentProject[]
  approvedInternships: ApprovedInternship[]
}

export type ResumeBuilderAction =
  | { type: 'SET_STEP'; payload: number }
  | { type: 'SET_TARGET_JD'; payload: { role: string; jd: string; careerFocusTag: string } }
  | { type: 'SET_CONTACT_INFO'; payload: ResumeContactInfo }
  | { type: 'TOGGLE_SUBJECT'; payload: string }
  | { type: 'TOGGLE_INTERNSHIP'; payload: number }
  | { type: 'TOGGLE_PROJECT_SELECTION'; payload: number }
  | { type: 'TOGGLE_CERTIFICATION_SELECTION'; payload: number }
  | { type: 'TOGGLE_AWARD_SELECTION'; payload: number }
  | { type: 'UPDATE_PROJECT'; payload: { index: number; project: UiProjectOverride } }
  | { type: 'ADD_PERSONAL_PROJECT'; payload: UiProjectOverride }
  | { type: 'DELETE_PERSONAL_PROJECT'; payload: number }
  | { type: 'ADD_CERTIFICATION'; payload: CertificationItem }
  | { type: 'UPDATE_CERTIFICATION'; payload: { index: number; certification: CertificationItem } }
  | { type: 'DELETE_CERTIFICATION'; payload: number }
  | { type: 'ADD_AWARD'; payload: AwardActivityItem }
  | { type: 'UPDATE_AWARD'; payload: { index: number; award: AwardActivityItem } }
  | { type: 'DELETE_AWARD'; payload: number }
  | { type: 'LINK_PROJECT_COURSES'; payload: { projectId: number; courseIds: string[] } }
  | { type: 'SET_LOADING_CONTEXT'; payload: boolean }
  | { type: 'SET_OPTIMIZING'; payload: boolean }
  | { type: 'SET_ZOOM'; payload: number | null }
  | { type: 'SET_CONTEXT_DATA'; payload: { studentInfo: ResumeBuilderState['studentInfo']; courses: EligibleCourse[]; projects: StudentProject[]; internships: ApprovedInternship[] } }
  | { type: 'PREPARE_PAYLOAD'; payload: PrepareResumePayloadRequestDto }
  | { type: 'SET_OPTIMIZED_RESULT'; payload: OptimizedResumeResponseDto }
  | { type: 'UPDATE_OPTIMIZED_SUMMARY'; payload: string }
  | { type: 'UPDATE_OPTIMIZED_SKILL'; payload: { group: OptimizedSkillGroup; index: number; description: string } }
  | { type: 'UPDATE_OPTIMIZED_BULLET'; payload: { section: OptimizedBulletSection; index: number; bulletIndex: number; value: string } }
  | { type: 'RESET' }
