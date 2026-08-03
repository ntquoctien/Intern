import { Button, Form, Input, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useEffect } from 'react'
import type { AwardActivityItem } from '../types'

interface AwardFormModalProps {
  open: boolean
  onClose: () => void
  onConfirm: (award: AwardActivityItem) => void
  initialAward?: AwardActivityItem | null
}

export function AwardFormModal({ open, onClose, onConfirm, initialAward }: AwardFormModalProps) {
  const [form] = Form.useForm()
  const isEditing = initialAward !== null && initialAward !== undefined

  useEffect(() => {
    if (!open) return
    if (isEditing && initialAward) {
      form.setFieldsValue({
        title: initialAward.title,
        organization: initialAward.organization,
        achievedDate: initialAward.achievedDate,
        description: initialAward.description,
      })
    } else {
      form.resetFields()
    }
  }, [open, isEditing, initialAward, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      onConfirm({
        id: initialAward?.id ?? Date.now(),
        title: values.title.trim(),
        organization: values.organization?.trim() || null,
        achievedDate: values.achievedDate?.trim() || null,
        description: values.description?.trim() || null,
        isSelectedForCv: initialAward?.isSelectedForCv ?? true,
      })
      form.resetFields()
      onClose()
    } catch {
      // Form validation failed.
    }
  }

  return (
    <Modal
      title={isEditing ? 'Sửa Thành tích / Hoạt động' : 'Thêm Thành tích / Hoạt động'}
      open={open}
      onCancel={onClose}
      width={640}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Hủy
        </Button>,
        <Button key="submit" type="primary" onClick={handleSubmit}>
          {isEditing ? 'Cập nhật' : 'Thêm'}
        </Button>,
      ]}
    >
      <Form form={form} layout="vertical" requiredMark="optional">
        <Form.Item
          label="Tên Thành tích / Hoạt động"
          name="title"
          rules={[{ required: true, message: 'Vui lòng nhập tên thành tích hoặc hoạt động' }]}
        >
          <Input placeholder="VD: Giải Nhì Cuộc thi Lập trình Hackathon 2025" />
        </Form.Item>

        <Form.Item label="Tổ chức / Đơn vị (tùy chọn)" name="organization">
          <Input placeholder="VD: Đại học CNTT" />
        </Form.Item>

        <Form.Item label="Thời gian đạt được (tùy chọn)" name="achievedDate">
          <Input placeholder="VD: 2025" />
        </Form.Item>

        <Form.Item label="Mô tả ngắn (tùy chọn)" name="description">
          <TextArea rows={4} placeholder="Tóm tắt bối cảnh, thành tích hoặc vai trò của bạn" />
        </Form.Item>
      </Form>
    </Modal>
  )
}