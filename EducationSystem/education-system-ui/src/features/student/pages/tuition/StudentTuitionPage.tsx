import { CalendarOutlined, CustomerServiceOutlined, InfoCircleOutlined, RightOutlined, WalletOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Empty, Spin, Tag, Typography } from 'antd'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { studentApi } from '../../studentApi'
import type { StudentTuition } from '../../types'

const amountFormat = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 })

export function StudentTuitionPage() {
  const query = useQuery({ queryKey: ['student', 'tuition'], queryFn: studentApi.tuition })
  const [selectedId, setSelectedId] = useState<string>()
  if (query.isLoading) return <div className="center-state"><Spin /></div>
  if (query.isError || !query.data) return <Alert type="error" showIcon message="Không thể tải thông tin học phí" />
  const items = query.data
  const selected = items.find(item => item.tuitionId === selectedId) ?? items[0]

  return <div className="student-tuition-page">
    <div className="student-page-heading"><Typography.Title level={2}>Học phí</Typography.Title><Typography.Text type="secondary">Thông tin học phí theo học kỳ.</Typography.Text></div>
    <Card className="tuition-scope-banner"><span><InfoCircleOutlined /></span><div><h3>Phạm vi thông tin</h3><p>Trang này hiển thị thông tin học phí theo học kỳ từ hệ thống hiện tại: số tiền cần nộp (Amount) và ngày đã thanh toán (PaidDate, nếu có).</p></div><WalletOutlined className="tuition-banner-art" /></Card>
    {items.length ? <div className="tuition-layout">
      <Card className="tuition-semesters-card" title="Danh sách học kỳ"><div>{items.map(item => <button className={selected?.tuitionId === item.tuitionId ? 'active' : ''} key={item.tuitionId} onClick={() => setSelectedId(item.tuitionId)}><span><CalendarOutlined /></span><div><b>HK {item.semester} ({item.academicYearName})</b><small>{dayjs(item.startDate).format('DD/MM/YYYY')} – {dayjs(item.endDate).format('DD/MM/YYYY')}</small></div><Tag color={item.isActivePlan ? 'blue' : 'green'}>{item.isActivePlan ? 'Đang học' : 'Đã kết thúc'}</Tag></button>)}</div></Card>
      {selected && <TuitionDetail item={selected} />}
    </div> : <TuitionEmpty />}
  </div>
}

function TuitionDetail({ item }: { item: StudentTuition }) {
  return <Card className="tuition-detail-card">
    <div className="tuition-detail-heading"><span><CalendarOutlined /></span><div><Typography.Title level={3}>HK {item.semester} ({item.academicYearName})</Typography.Title><Typography.Text>{dayjs(item.startDate).format('DD/MM/YYYY')} – {dayjs(item.endDate).format('DD/MM/YYYY')}</Typography.Text></div><Tag color={item.isActivePlan ? 'blue' : 'green'}>{item.isActivePlan ? 'Đang học' : 'Đã kết thúc'}</Tag></div>
    <section className="tuition-values"><div><small>Số tiền học phí</small><strong>{item.rawAmount == null ? '—' : amountFormat.format(item.rawAmount)}</strong><span>(Giá trị ghi nhận)</span></div><div><small>Ngày thanh toán</small><strong className="paid-date">{item.paidDate ? dayjs(item.paidDate).format('DD/MM/YYYY') : '—'}</strong><span>(Nếu có)</span></div><div><small>Đơn vị tiền tệ</small><strong>—</strong><span>(Chưa xác nhận)</span></div></section>
    <section className="tuition-notes"><h4>Ghi chú</h4><ul><li>Số tiền hiển thị là giá trị ghi nhận từ hệ thống.</li><li>Để biết chi tiết về hạn thanh toán, các khoản thu, miễn giảm hoặc giao dịch, vui lòng liên hệ Phòng Tài chính – Kế toán của Nhà trường.</li></ul></section>
    <section className="tuition-help"><span><CustomerServiceOutlined /></span><div><b>Cần hỗ trợ về học phí?</b><small>Vui lòng liên hệ Phòng Tài chính – Kế toán để được hỗ trợ.</small></div><Link to="/student/form-requests">Hướng dẫn liên hệ <RightOutlined /></Link></section>
  </Card>
}

function TuitionEmpty() {
  return <Card className="tuition-empty-card"><Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={<div><h3>Chưa có dữ liệu học phí trong hệ thống</h3><p>Hiện chưa có thông tin học phí cho các học kỳ của bạn.<br />Vui lòng quay lại sau hoặc liên hệ Phòng Tài chính – Kế toán nếu cần hỗ trợ.</p><Link to="/student/form-requests"><Button type="primary">Liên hệ hỗ trợ</Button></Link></div>} /></Card>
}
