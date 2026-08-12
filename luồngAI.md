CV Optimization Flow - LLM Output Path
1. Frontend Initiates Request

A4PaperPreview.tsx 
→ useResumeStore (calls optimizeResume action)
→ POST /api/career/resume/optimize
→ PrepareResumePayloadRequestDto (with targetRole, JD, selected courses/projects)
2. Backend Receives & Hydrates Context

ResumeOptimizationController.cs
→ HydrateResumeContextAsync (combines DB data + user input)
→ Creates ResumePromptPayloadDto with:
   - Student info (name, major, GPA)
   - Matched subjects (top-K similar courses via vector search)
   - Projects & internships
   - Certifications & awards
→ Passes to LLM service
3. LLM Processing

LlmResumeGeneratorService.GenerateOptimizedResumeAsync()
→ Calls Gemini / Groq / provider API with:
   - systemInstruction (our updated prompt)
   - userPrompt (JSON payload with all context)
   - responseFormat (JSON schema for structured output)
→ LLM returns raw JSON string
4. LLM Output Format (returned as raw JSON)

{
  "header": {
    "fullName": "Nguyễn Văn A",
    "studentCode": "SV001",
    "majorName": "Công Nghệ Thông Tin",
    "gpa": 7.84,
    "targetRole": "Backend Developer"
  },
  "education": {
    "institutionName": "Đại học Bách Khoa TP.HCM",
    "majorName": "Công Nghệ Thông Tin",
    "degreeName": "Cử nhân",
    "gpa": 7.84,
    "durationText": "2022 - 2026"
  },
  "professionalSummary": "Lập trình viên Backend có định hướng phát triển hệ thống web/API hiệu năng cao. Thành thạo C#, .NET 8, SQL Server và REST API, với kinh nghiệm xây dựng các hệ thống distributed. Mong muốn đóng góp cho sản phẩm thực tế của doanh nghiệp.",
  "skills": {
    "knowledgeDomain": [
      {
        "skillName": "Lập trình Backend",
        "proficiency": "Thành thạo",
        "keywords": ["C#", ".NET 8", "SQL Server", "Entity Framework"]
      },
      {
        "skillName": "Thiết kế API",
        "proficiency": "Khá tốt",
        "keywords": ["REST API", "OpenAPI", "JWT", "Microservices"]
      }
    ],
    "functionalSkills": [...],
    "interpersonalSkills": [...]
  },
  "projects": [
    {
      "projectId": 1,
      "projectName": "Education Management System",
      "techStack": "C#, .NET 8, SQL Server, React",
      "myRole": "Backend Developer",
      "actionBulletPoints": [
        "Xây dựng 6 microservices độc lập với kiến trúc Schema-per-Service",
        "Tối ưu hóa database queries giảm response time 40%",
        "Triển khai JWT authentication & role-based authorization"
      ]
    }
  ],
  "internships": [...],
  "certifications": ["AWS Solutions Architect", "Docker Certified Associate"],
  "awardsAndActivities": ["Dean's List 2024", "3rd Prize AI Hackathon"],
  "qualityMetrics": {
    "jobAlignmentScore": 0.87,
    "contentPreservationScore": 0.92,
    "hasHallucinationWarning": false
  }
}
5. Backend Post-Processing (Fact Guard)

After LLM returns, ResumeOptimizationController applies OVERRIDES:

✓ Lock Education data:
  - MajorName, Gpa, InstitutionName from DB (không cho LLM thay đổi)
  - DurationText từ enrollment records

✓ Lock Header:
  - FullName, StudentCode từ Identity service
  - TargetRole từ user input (không cho LLM tạo)
  - REMOVE MajorName & GPA (bị trùng với Education section)

✓ Lock Project/Internship:
  - ProjectName, CompanyName, Position từ DB
  - TechStack từ student input
  - Chỉ cho LLM generate: actionBulletPoints

✓ Validate:
  - actionBulletPoints phải có 2-3 items
  - Keywords phải từ recognized tech list
  - Professional summary phải 3 câu
  - No extra data (hallucinations)
6. Return to Frontend

OptimizedResumeResponseDto (C# DTO)
→ JSON serialize (asp.net core)
→ ApiResponse<OptimizedResumeResponseDto>
→ HTTP 200 OK

{
  "success": true,
  "data": { ...full CV object... },
  "message": "Vietnamese ATS resume generated."
}
7. Frontend Receives & Renders

A4PaperPreview.tsx
→ setOptimizedCvResult(data)
→ Renders OptimizedCvContent component
→ Maps over skills → renderSkillGroup() 
→ Displays inline text rows (NOT badges anymore)
→ User can edit summary/bullets directly (contentEditable)
→ Print to PDF
Key Points:
Stage	What Happens	Data Source
LLM Input	Receives context payload (student data + courses + projects)	DB + User selections
LLM Processing	Generates natural language CV content	AI model
LLM Output	Returns structured JSON with skills, bullets, summary	Gemini / Groq API
Post-Guard	Overrides LLM output with verified DB data	Database (source of truth)
Frontend Render	Displays clean inline skill rows + editable content	Validated DTO