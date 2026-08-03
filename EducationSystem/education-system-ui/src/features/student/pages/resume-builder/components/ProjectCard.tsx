import { DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { Button, Card, Checkbox, Space, Typography, Select, message } from 'antd'
import type { UiProjectOverride, EligibleCourse } from '../types'

const { Text } = Typography

type ProjectCardProps = {
  project: UiProjectOverride
  isSelected: boolean
  onSelect: (projectId: number, selected: boolean) => void
  onEdit?: (projectId: number) => void
  onDelete?: (projectId: number) => void
  eligibleCourses: EligibleCourse[]
  onMapCourse?: (projectId: number, courseId: string, courseCode: string, courseName: string) => void
  showEditButton?: boolean
  showDeleteButton?: boolean
}

export function ProjectCard({
  project,
  isSelected,
  onSelect,
  onEdit,
  onDelete,
  eligibleCourses,
  onMapCourse,
  showEditButton = true,
  showDeleteButton = false,
}: ProjectCardProps) {
  const handleMapCourse = (subjectId: string) => {
    const course = eligibleCourses.find(c => c.subjectId === subjectId)
    if (course && onMapCourse) {
      onMapCourse(project.projectId, subjectId, course.subjectCode, course.subjectName)
      message.success(`Đã liên kết với môn ${course.subjectName}`)
    }
  }

  return (
    <Card size="small">
      <Space direction="vertical" style={{ width: '100%' }} size="small">
        <div style={{ display: 'flex', gap: '12px', alignItems: 'flex-start' }}>
          <Checkbox
            checked={isSelected}
            onChange={(e) => onSelect(project.projectId, e.target.checked)}
            style={{ marginTop: '4px' }}
          />
          <div style={{ flex: 1 }}>
            <Text strong>{project.projectName}</Text>
            <br />
            <Text type="secondary">{project.myRole}</Text>
            <br />
            <Text type="secondary">{project.techStack}</Text>
            {project.myContributions && (
              <>
                <br />
                <Text type="secondary">{project.myContributions}</Text>
              </>
            )}
          </div>
          <div style={{ display: 'flex', gap: '8px' }}>
            {showEditButton && onEdit && (
              <Button
                type="text"
                size="small"
                icon={<EditOutlined />}
                onClick={() => onEdit(project.projectId)}
              />
            )}
            {showDeleteButton && onDelete && (
              <Button
                type="text"
                size="small"
                danger
                icon={<DeleteOutlined />}
                onClick={() => onDelete(project.projectId)}
              />
            )}
          </div>
        </div>

        {/* Course mapping for school projects */}
        {onMapCourse && eligibleCourses.length > 0 && (
          <div style={{ marginLeft: '28px' }}>
            <Select
              placeholder="Liên kết với môn học (tùy chọn)"
              allowClear
              value={project.mappedCourseId || undefined}
              onChange={handleMapCourse}
              style={{ width: '100%' }}
              options={eligibleCourses.map(course => ({
                label: `${course.subjectCode} - ${course.subjectName}`,
                value: course.subjectId,
              }))}
            />
            {project.mappedCourseId && (
              <Text type="success" style={{ marginTop: '8px', display: 'block' }}>
                ✓ Đã liên kết: {project.mappedCourseName}
              </Text>
            )}
          </div>
        )}
      </Space>
    </Card>
  )
}

