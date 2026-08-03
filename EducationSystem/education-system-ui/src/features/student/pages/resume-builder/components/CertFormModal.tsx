import { Button, Form, Input, Modal } from 'antd'
import { useEffect } from 'react'
import type { CertificationItem } from '../types'

interface CertFormModalProps {
  open: boolean
  onClose: () => void
  onConfirm: (certification: CertificationItem) => void
  initialCertification?: CertificationItem | null
}

export function CertFormModal({ open, onClose, onConfirm, initialCertification }: CertFormModalProps) {
  const [form] = Form.useForm()
  const isEditing = initialCertification !== null && initialCertification !== undefined

  useEffect(() => {
    if (!open) return
    if (isEditing && initialCertification) {
      form.setFieldsValue({
        name: initialCertification.name,
        issuer: initialCertification.issuer,
        issueDate: initialCertification.issueDate,
        expirationDate: initialCertification.expirationDate,
        credentialUrl: initialCertification.credentialUrl,
      })
    } else {
      form.resetFields()
    }
  }, [open, isEditing, initialCertification, form])

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      onConfirm({
        id: initialCertification?.id ?? Date.now(),
        name: values.name.trim(),
        issuer: values.issuer.trim(),
        issueDate: values.issueDate?.trim() || null,
        expirationDate: values.expirationDate?.trim() || null,
        credentialUrl: values.credentialUrl?.trim() || null,
        isSelectedForCv: initialCertification?.isSelectedForCv ?? true,
      })
      form.resetFields()
      onClose()
    } catch {
      // Form validation failed.
    }
  }

  return (
    <Modal
      title={isEditing ? 'Sửa Chứng chỉ' : 'Thêm Chứng chỉ'}
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
          label="Tên Chứng chỉ"
          name="name"
          rules={[{ required: true, message: 'Vui lòng nhập tên chứng chỉ' }]}
        >
          <Input placeholder="VD: AWS Certified Developer - Associate" />
        </Form.Item>

        <Form.Item
          label="Tổ chức cấp"
          name="issuer"
          rules={[{ required: true, message: 'Vui lòng nhập tổ chức cấp' }]}
        >
          <Input placeholder="VD: Amazon Web Services" />
        </Form.Item>

        <Form.Item label="Ngày cấp (tùy chọn)" name="issueDate">
          <Input placeholder="VD: 10/2025" />
        </Form.Item>

        <Form.Item label="Ngày hết hạn (tùy chọn)" name="expirationDate">
          <Input placeholder="VD: 10/2028" />
        </Form.Item>

        <Form.Item label="URL xác thực (tùy chọn)" name="credentialUrl">
          <Input placeholder="VD: https://aws.amazon.com/verify/..." type="url" />
        </Form.Item>
      </Form>
    </Modal>
  )
}