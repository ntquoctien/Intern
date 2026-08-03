import { PlusOutlined } from '@ant-design/icons'
import { Button, Card, Checkbox, Empty, Modal, Space, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { useResumeStore } from '../hooks/useResumeStore'
import { formatAwardLabel, formatCertificationLabel } from '../utils/resumeSelectionFormatters'
import { AwardFormModal } from './AwardFormModal'
import { CertFormModal } from './CertFormModal'

const { Text, Title } = Typography

export function CertAndAwardManager() {
  const {
    state,
    toggleCertificationSelection,
    toggleAwardSelection,
    addCertification,
    updateCertification,
    deleteCertification,
    addAward,
    updateAward,
    deleteAward,
  } = useResumeStore()
  const { certifications, awardsAndActivities } = state
  const [certFormOpen, setCertFormOpen] = useState(false)
  const [awardFormOpen, setAwardFormOpen] = useState(false)
  const [editingCertificationId, setEditingCertificationId] = useState<number | null>(null)
  const [editingAwardId, setEditingAwardId] = useState<number | null>(null)

  const editingCertification = useMemo(
    () => certifications.find(item => item.id === editingCertificationId) ?? null,
    [certifications, editingCertificationId],
  )
  const editingAward = useMemo(
    () => awardsAndActivities.find(item => item.id === editingAwardId) ?? null,
    [awardsAndActivities, editingAwardId],
  )

  const handleSaveCertification = (item: Parameters<typeof addCertification>[0]) => {
    const index = certifications.findIndex(certification => certification.id === item.id)
    if (index >= 0) {
      updateCertification(index, item)
    } else {
      addCertification(item)
    }
  }

  const handleSaveAward = (item: Parameters<typeof addAward>[0]) => {
    const index = awardsAndActivities.findIndex(award => award.id === item.id)
    if (index >= 0) {
      updateAward(index, item)
    } else {
      addAward(item)
    }
  }

  const confirmDelete = (title: string, onOk: () => void) => {
    Modal.confirm({
      title: `Xóa ${title}?`,
      content: 'Thao tác này sẽ xóa mục khỏi danh sách chọn CV.',
      okText: 'Xóa',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk,
    })
  }

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Card size="small" className="resume-section-card">
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Space style={{ width: '100%', justifyContent: 'space-between' }} align="center">
            <div>
              <Title level={5} style={{ margin: 0 }}>Chứng chỉ Chuyên môn</Title>
              <Text type="secondary">Chọn các chứng chỉ thật sự phù hợp với CV và JD.</Text>
            </div>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setCertFormOpen(true)}>
              + Thêm Chứng chỉ
            </Button>
          </Space>

          {certifications.length === 0 ? (
            <Empty description="Chưa có chứng chỉ nào" image={Empty.PRESENTED_IMAGE_SIMPLE} />
          ) : (
            certifications.map(certification => (
              <Card key={certification.id} size="small" className="resume-item-card">
                <Space direction="vertical" size="small" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                    <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start', flex: 1 }}>
                      <Checkbox
                        checked={certification.isSelectedForCv}
                        onChange={() => toggleCertificationSelection(certification.id)}
                      />
                      <div style={{ flex: 1 }}>
                        <Text strong>{formatCertificationLabel(certification)}</Text>
                        <br />
                        <Text type="secondary">
                          {certification.issueDate ? `Cấp: ${certification.issueDate}` : 'Cấp: Chưa bổ sung'}
                          {certification.expirationDate ? ` • Hết hạn: ${certification.expirationDate}` : ''}
                        </Text>
                        {certification.credentialUrl && (
                          <>
                            <br />
                            <Text type="secondary">URL: {certification.credentialUrl}</Text>
                          </>
                        )}
                        <br />
                        <Text type="secondary">
                          {certification.isSelectedForCv ? '[☑ Đưa vào CV]' : '[ ] Đưa vào CV'}
                        </Text>
                      </div>
                    </div>
                    <Space size="small">
                      <Button size="small" onClick={() => {
                        setEditingCertificationId(certification.id)
                        setCertFormOpen(true)
                      }}>
                        Sửa ✎
                      </Button>
                      <Button
                        size="small"
                        danger
                        onClick={() => confirmDelete(formatCertificationLabel(certification), () => deleteCertification(certification.id))}
                      >
                        Xóa 🗑
                      </Button>
                    </Space>
                  </div>
                </Space>
              </Card>
            ))
          )}
        </Space>
      </Card>

      <Card size="small" className="resume-section-card">
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Space style={{ width: '100%', justifyContent: 'space-between' }} align="center">
            <div>
              <Title level={5} style={{ margin: 0 }}>Giải thưởng & Hoạt động</Title>
              <Text type="secondary">Thêm thành tích, hoạt động CLB hoặc học bổng có thể kiểm chứng.</Text>
            </div>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setAwardFormOpen(true)}>
              + Thêm Thành tích / Hoạt động
            </Button>
          </Space>

          {awardsAndActivities.length === 0 ? (
            <Empty description="Chưa có thành tích / hoạt động nào" image={Empty.PRESENTED_IMAGE_SIMPLE} />
          ) : (
            awardsAndActivities.map(award => (
              <Card key={award.id} size="small" className="resume-item-card">
                <Space direction="vertical" size="small" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, alignItems: 'flex-start' }}>
                    <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start', flex: 1 }}>
                      <Checkbox
                        checked={award.isSelectedForCv}
                        onChange={() => toggleAwardSelection(award.id)}
                      />
                      <div style={{ flex: 1 }}>
                        <Text strong>{formatAwardLabel(award)}</Text>
                        <br />
                        <Text type="secondary">
                          {award.achievedDate ? `Đạt được: ${award.achievedDate}` : 'Đạt được: Chưa bổ sung'}
                        </Text>
                        {award.description && (
                          <>
                            <br />
                            <Text type="secondary">{award.description}</Text>
                          </>
                        )}
                        <br />
                        <Text type="secondary">
                          {award.isSelectedForCv ? '[☑ Đưa vào CV]' : '[ ] Đưa vào CV'}
                        </Text>
                      </div>
                    </div>
                    <Space size="small">
                      <Button size="small" onClick={() => {
                        setEditingAwardId(award.id)
                        setAwardFormOpen(true)
                      }}>
                        Sửa ✎
                      </Button>
                      <Button
                        size="small"
                        danger
                        onClick={() => confirmDelete(formatAwardLabel(award), () => deleteAward(award.id))}
                      >
                        Xóa 🗑
                      </Button>
                    </Space>
                  </div>
                </Space>
              </Card>
            ))
          )}
        </Space>
      </Card>

      <CertFormModal
        open={certFormOpen}
        onClose={() => {
          setCertFormOpen(false)
          setEditingCertificationId(null)
        }}
        onConfirm={handleSaveCertification}
        initialCertification={editingCertification}
      />

      <AwardFormModal
        open={awardFormOpen}
        onClose={() => {
          setAwardFormOpen(false)
          setEditingAwardId(null)
        }}
        onConfirm={handleSaveAward}
        initialAward={editingAward}
      />
    </Space>
  )
}