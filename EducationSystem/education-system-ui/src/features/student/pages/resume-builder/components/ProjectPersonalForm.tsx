import { Button, Form, Input, Modal, Radio, Select, Space } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useEffect } from 'react'
import type { EligibleCourse, UiProjectOverride } from '../types'
import { createTemporaryProjectId } from '../utils/localIds'

interface ProjectPersonalFormProps {
  open: boolean
  onClose: () => void
  onConfirm: (project: UiProjectOverride) => void
  initialProject?: UiProjectOverride | null
  eligibleCourses: EligibleCourse[]
}

export function ProjectPersonalForm({
  open,
  onClose,
  onConfirm,
  initialProject,
  eligibleCourses,
}: ProjectPersonalFormProps) {
  const [form] = Form.useForm()
  const isEditing = initialProject !== null && initialProject !== undefined

  useEffect(() => {
    if (open) {
      if (isEditing && initialProject) {
        form.setFieldsValue({
          projectName: initialProject.projectName,
          techStack: initialProject.techStack,
          sourceCodeUrl: initialProject.sourceCodeUrl,
          teamSize: initialProject.teamSize,
          myRole: initialProject.myRole,
          mappedCourseId: initialProject.mappedCourseId,
          myContributions: initialProject.myContributions,
        })
      } else {
        form.resetFields()
      }
    }
  }, [open, isEditing, initialProject, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()

      // Get course details if mappedCourseId is selected
      let mappedCourseCode: string | undefined
      let mappedCourseName: string | undefined
      if (values.mappedCourseId) {
        const selectedCourse = eligibleCourses.find(c => c.subjectId === values.mappedCourseId)
        if (selectedCourse) {
          mappedCourseCode = selectedCourse.subjectCode
          mappedCourseName = selectedCourse.subjectName
        }
      }

      const newProject: UiProjectOverride = {
        projectId: isEditing && initialProject ? initialProject.projectId : createTemporaryProjectId(),
        projectName: values.projectName,
        techStack: values.techStack,
        sourceCodeUrl: values.sourceCodeUrl || null,
        teamSize: values.teamSize,
        myRole: values.myRole,
        mappedCourseId: values.mappedCourseId,
        mappedCourseCode,
        mappedCourseName,
        myContributions: values.myContributions,
        source: 'personal',
        linkedCourseIds: [],
      }
      onConfirm(newProject)
      form.resetFields()
      onClose()
    } catch {
      // Validation failed
    }
  }

  return (
    <Modal
      title={isEditing ? 'Chỉnh sửa Dự án Cá nhân' : 'Thêm Dự án Cá nhân'}
      open={open}
      onCancel={onClose}
      width={700}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Hủy
        </Button>,
        <Button key="confirm" type="primary" onClick={handleSubmit}>
          {isEditing ? 'Cập nhật' : 'Thêm'}
        </Button>,
      ]}
    >
      <Form form={form} layout="vertical" requiredMark="optional">
        <Form.Item
          label="Tên Dự án"
          name="projectName"
          rules={[{ required: true, message: 'Vui lòng nhập tên dự án' }]}
        >
          <Input placeholder="VD: E-Commerce Platform" />
        </Form.Item>

        <Form.Item
          label="Công Nghệ Sử Dụng"
          name="techStack"
          rules={[{ required: true, message: 'Vui lòng nhập công nghệ' }]}
        >
          <Input placeholder="VD: React, Node.js, PostgreSQL" />
        </Form.Item>

        <Form.Item
          label="Link Source Code / GitHub (Tùy chọn)"
          name="sourceCodeUrl"
        >
          <Input placeholder="VD: https://github.com/username/repo" type="url" />
        </Form.Item>

        <Form.Item label="Loại Dự án" name="teamSize" initialValue={1}>
          <Radio.Group>
            <Space direction="vertical">
              <Radio value={1}>👤 Cá nhân (1 người)</Radio>
              <Radio value={2}>👥 Nhóm (2+ người)</Radio>
            </Space>
          </Radio.Group>
        </Form.Item>

        <Form.Item
          label="Vai Trò Của Bạn"
          name="myRole"
          rules={[{ required: true, message: 'Vui lòng nhập vai trò' }]}
        >
          <Input placeholder="VD: Backend Developer, Team Leader" />
        </Form.Item>

        <Form.Item
          label="Môn Học Gán Kèm (Tùy chọn)"
          name="mappedCourseId"
        >
          <Select
            placeholder="Chọn môn học liên quan..."
            allowClear
            options={eligibleCourses.map(c => ({
              label: `${c.subjectCode} - ${c.subjectName}`,
              value: c.subjectId,
            }))}
          />
        </Form.Item>

        <Form.Item
          label="Đóng Góp & Kết Quả"
          name="myContributions"
          rules={[{ required: true, message: 'Vui lòng mô tả đóng góp' }]}
        >
          <TextArea
            placeholder="Mô tả chi tiết những gì bạn đã làm, kết quả đạt được..."
            rows={4}
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}
