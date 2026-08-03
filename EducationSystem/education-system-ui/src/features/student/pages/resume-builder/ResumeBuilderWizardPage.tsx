import { FileTextOutlined, HolderOutlined, RobotOutlined, SettingOutlined } from '@ant-design/icons'
import { Steps, Typography } from 'antd'
import './resume-builder.css'
import { ResumeStoreProvider, useResumeStore } from './hooks/useResumeStore'
import { Step1TargetJd } from './components/Step1TargetJd'
import { Step2CoursesOutcome } from './components/Step2CoursesOutcome'
import { Step3EvidenceOverride } from './components/Step3EvidenceOverride'
import { Step4PreviewExport } from './components/Step4PreviewExport'
import { A4PaperPreview } from './components/A4PaperPreview'

const { Title, Text } = Typography

const steps = [
  { title: 'Mục tiêu & JD', icon: <FileTextOutlined /> },
  { title: 'Học phần & CLO', icon: <HolderOutlined /> },
  { title: 'Dự án & Thực tập', icon: <SettingOutlined /> },
  { title: 'Tối ưu AI & Xuất PDF', icon: <RobotOutlined /> },
]

function WizardContent() {
  const { state } = useResumeStore()
  const { currentStep } = state

  const renderStep = () => {
    switch (currentStep) {
      case 0:
        return <Step1TargetJd />
      case 1:
        return <Step2CoursesOutcome />
      case 2:
        return <Step3EvidenceOverride />
      case 3:
        return <Step4PreviewExport />
      default:
        return <Step1TargetJd />
    }
  }

  return (
    <div className="resume-builder-wizard">
      <div className="wizard-header">
        <Title level={3}>Smart Resume Builder</Title>
        <Text type="secondary">Tạo CV chuyên nghiệp với sự hỗ trợ của AI</Text>
      </div>

      <div className="wizard-steps-container">
        <Steps
          current={currentStep}
          items={steps.map(s => ({ title: s.title, icon: s.icon }))}
          className="wizard-steps"
        />
      </div>

      <div className="wizard-content">
        <div className="wizard-left-panel">
          {renderStep()}
        </div>
        <div className="wizard-right-panel">
          <A4PaperPreview />
        </div>
      </div>
    </div>
  )
}

export function ResumeBuilderWizardPage() {
  return (
    <ResumeStoreProvider>
      <WizardContent />
    </ResumeStoreProvider>
  )
}
