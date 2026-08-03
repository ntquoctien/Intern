import { BookOutlined, CheckCircleOutlined, RiseOutlined, StarFilled } from '@ant-design/icons'
import { Badge, Button, Card, Checkbox, Empty, Space, Tag, Typography } from 'antd'
import { useResumeStore } from '../hooks/useResumeStore'
import type { CloLevel } from '../types'

const { Title, Text } = Typography

const CLO_CONFIG: Record<CloLevel, { color: string; label: string; description: string }> = {
  E: { color: 'green', label: 'E', description: 'Nền tảng' },
  R: { color: 'gold', label: 'R', description: 'Củng cố' },
  D: { color: 'blue', label: 'D', description: 'Thành thục' },
}

export function Step2CoursesOutcome() {
  const { state, toggleSubjectSelection, setStep } = useResumeStore()
  const { eligibleCourses, selectedSubjectIds } = state

  const sortedCourses = [...eligibleCourses].sort((a, b) =>
    (b.similarityScore ?? 0) - (a.similarityScore ?? 0) || b.score - a.score,
  )
  const hasVectorRecommendations = sortedCourses.some(
    course => course.recommendationSource === 'vector',
  )

  return (
    <div className="step-courses-outcome">
      <Card className="step-card">
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <div className="step-header">
            <BookOutlined className="step-icon" />
            <div>
              <Title level={4} style={{ margin: 0 }}>Học phần phù hợp với JD</Title>
              <Text type="secondary">
                {hasVectorRecommendations
                  ? 'AI xếp hạng tối đa 10 môn gần nhất với JD — 3 môn đầu được chọn sẵn'
                  : 'Chưa có kết quả khớp JD — đang hiển thị tối đa 10 môn điểm cao để tham khảo'}
              </Text>
            </div>
          </div>

          <div className="clo-legend">
            <Text strong>Mức độ CLO:</Text>
            <Space size="middle" wrap>
              {Object.entries(CLO_CONFIG).map(([key, config]) => (
                <Tag key={key} color={config.color}>
                  {config.label} - {config.description}
                </Tag>
              ))}
            </Space>
          </div>

          {sortedCourses.length === 0 ? (
            <Empty
              description="Không tìm thấy môn học phù hợp với JD hiện tại. Hãy bổ sung mô tả công việc cụ thể hơn."
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          ) : (
            <div className="course-list">
              {sortedCourses.map((course, index) => {
                const cloConfig = course.cloLevel ? CLO_CONFIG[course.cloLevel] : null
                const isSelected = selectedSubjectIds.includes(course.subjectId)
                const relevance = course.similarityScore !== undefined
                  ? Math.round(course.similarityScore * 100)
                  : null
                const recommendationLabel = course.recommendationSource === 'score-fallback'
                  ? 'Điểm cao'
                  : index < 3
                    ? 'Rất phù hợp'
                    : index < 7
                      ? 'Phù hợp'
                      : 'Tham khảo'

                return (
                  <div
                    key={course.subjectId}
                    className={`course-item ${isSelected ? 'selected' : ''}`}
                    onClick={() => toggleSubjectSelection(course.subjectId)}
                  >
                    <Checkbox checked={isSelected} />
                    <div className={`course-rank ${index < 3 ? 'top' : ''}`}>
                      {index < 3 ? <StarFilled /> : index + 1}
                    </div>
                    <div className="course-info">
                      <div className="course-header">
                        <div className="course-title">
                          <Text strong>{course.subjectCode}</Text>
                          <Text className="course-name">{course.subjectName}</Text>
                        </div>
                        <div className="course-recommendation-tags">
                          <Tag color={course.recommendationSource === 'score-fallback' ? 'gold' : index < 3 ? 'blue' : index < 7 ? 'cyan' : 'default'}>
                            {recommendationLabel}
                          </Tag>
                          {cloConfig && (
                            <Badge
                              count={cloConfig.label}
                              style={{
                                backgroundColor: cloConfig.color === 'green'
                                  ? '#52c41a'
                                  : cloConfig.color === 'gold'
                                    ? '#faad14'
                                    : '#1677ff',
                              }}
                              title={cloConfig.description}
                            />
                          )}
                        </div>
                      </div>

                      <div className="course-meta">
                        <Text type="secondary">Điểm học phần: {course.score.toFixed(1)}</Text>
                        {relevance !== null ? (
                          <Text className="course-relevance">
                            <RiseOutlined /> Khớp JD {relevance}%
                          </Text>
                        ) : (
                          <Text type="secondary">Xếp hạng theo điểm học tập</Text>
                        )}
                      </div>

                      {relevance !== null && (
                        <div className="course-relevance-bar" aria-label={`Độ phù hợp ${relevance}%`}>
                          <span style={{ width: `${Math.max(relevance, 4)}%` }} />
                        </div>
                      )}

                      {(course.matchedOutcomes?.length ?? 0) > 0 && (
                        <div className="matched-outcomes">
                          <Text className="matched-outcomes-title">Vì sao phù hợp với JD:</Text>
                          {course.matchedOutcomes.slice(0, 2).map((outcome, outcomeIndex) => (
                            <div
                              key={`${outcome.code}-${outcomeIndex}`}
                              className="matched-outcome"
                            >
                              <Tag color="geekblue">{outcome.code}</Tag>
                              <span>{outcome.description}</span>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          )}

          <div className="step-summary">
            <Text type="secondary">
              Đã chọn <strong>{selectedSubjectIds.length}</strong> / {sortedCourses.length} môn được đề xuất
            </Text>
          </div>

          <div className="step-actions">
            <Button onClick={() => setStep(0)}>← Quay lại</Button>
            <Button
              type="primary"
              size="large"
              icon={<CheckCircleOutlined />}
              onClick={() => setStep(2)}
              disabled={selectedSubjectIds.length === 0}
            >
              Tiếp tục ➔ Chọn Minh chứng Thực tế
            </Button>
          </div>
        </Space>
      </Card>
    </div>
  )
}
