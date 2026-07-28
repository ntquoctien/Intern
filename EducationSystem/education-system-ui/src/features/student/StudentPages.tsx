import { useQuery } from '@tanstack/react-query'
import { Alert, Avatar, Button, Calendar as AntCalendar, Card, DatePicker, Empty, Input, Segmented, Select, Space, Spin, Statistic, Tag, Typography } from 'antd'
import { ApartmentOutlined, BankOutlined, BookOutlined, CalendarOutlined, CheckCircleOutlined, ClockCircleOutlined, CloseCircleOutlined, DownOutlined, FileTextOutlined, FilterOutlined, FormOutlined, IdcardOutlined, InfoCircleOutlined, LeftOutlined, ReadOutlined, RightOutlined, SafetyCertificateOutlined, SearchOutlined, SolutionOutlined, TrophyOutlined, UnorderedListOutlined, UpOutlined, UserOutlined, WarningOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import { useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { studentApi } from './studentApi'
import type { Attendance, ExamResult, FormRequest, ScheduleItem, StudentFormTemplate, StudentSubject } from './types'

function QueryState<T>({ query, empty, children }: { query: { isLoading: boolean; isError: boolean; data?: T }; empty?: boolean; children: (data: T) => ReactNode }) {
  if (query.isLoading) return <div className="center-state"><Spin description="Đang tải dữ liệu..." /></div>
  if (query.isError) return <Alert type="error" showIcon message="Không thể tải dữ liệu" description="Vui lòng thử lại hoặc liên hệ quản trị viên nếu lỗi tiếp diễn." />
  if (!query.data || empty) return <Empty description="Chưa có dữ liệu" />
  return <>{children(query.data)}</>
}

const raw = (value?: number | null) => value == null ? 'Chưa xác định' : String(value)
const dateTime = (value: string) => dayjs(value).format('DD/MM/YYYY HH:mm')

export function StudentDashboardPage() {
  const query = useQuery({ queryKey: ['student', 'dashboard'], queryFn: async () => {
    const [profile, subjects, schedule, results] = await Promise.all([studentApi.profile(), studentApi.subjects(), studentApi.schedule(), studentApi.examResults()])
    return { profile, subjects, schedule, results }
  } })
  return <QueryState query={query}>{({ profile, subjects, schedule, results }) => {
    const upcoming = [...schedule].filter(item => dayjs(item.endDateTime).isAfter(dayjs())).sort((a, b) => dayjs(a.startDateTime).valueOf() - dayjs(b.startDateTime).valueOf()).slice(0, 4)
    const exams = [...results].sort((a, b) => dayjs(a.examStartDate).valueOf() - dayjs(b.examStartDate).valueOf()).slice(0, 3)
    const totalCredits = subjects.reduce((sum, item) => sum + item.creditPoint, 0)
    return <div className="student-dashboard">
      <section className="student-metrics">
        <DashboardMetric icon={<BookOutlined />} label="Lớp đã ghi danh" value={subjects.length} note="Đang học" tone="blue" />
        <DashboardMetric icon={<CheckCircleOutlined />} label="Buổi học" value={schedule.length} note="Trong lịch hiện có" tone="green" />
        <DashboardMetric icon={<FileTextOutlined />} label="Tổng tín chỉ" value={totalCredits} note="Đã đăng ký" tone="purple" />
        <DashboardMetric icon={<TrophyOutlined />} label="Kết quả thi" value={results.length} note="Bản ghi hiện có" tone="orange" />
      </section>
      <div className="student-dashboard-layout">
        <div className="student-dashboard-main">
          <div className="student-dashboard-grid">
            <DashboardPanel title="Lịch học sắp tới" link="/student/schedule" linkText="Xem thời khóa biểu">
              {upcoming.length ? <div className="upcoming-schedule">{upcoming.map(item => <Link to="/student/schedule" key={item.scheduleId} className="upcoming-row">
                <time><small>{dayjs(item.startDateTime).format('dddd')}</small><b>{dayjs(item.startDateTime).format('DD')}</b><span>Tháng {dayjs(item.startDateTime).format('M')}</span></time>
                <div><span>{dayjs(item.startDateTime).format('HH:mm')} - {dayjs(item.endDateTime).format('HH:mm')}</span><b>{item.subjectName}</b><small>{item.subjectCode} · {item.className}</small></div>
                <div className="schedule-room"><b>{item.roomName ?? 'Chưa có phòng'}</b><small>{item.teacherName ?? 'Chưa có giảng viên'}</small></div><RightOutlined />
              </Link>)}</div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không có lịch sắp tới" />}
            </DashboardPanel>
            <DashboardPanel title="Thông báo mới" link="/student/announcements" linkText="Xem tất cả">
              <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chưa có thông báo mới" />
            </DashboardPanel>
          </div>
          <div className="student-dashboard-grid lower">
            <DashboardPanel title="Lớp học phần đang học" link="/student/subjects" linkText="Xem tất cả">
              <div className="subject-compact-list">{subjects.slice(0, 6).map((item, index) => <Link to="/student/subjects" key={item.enrollmentId}><Tag color={['blue', 'green', 'purple', 'orange'][index % 4]}>{item.subjectCode}</Tag><span>{item.subjectName}</span><small>{item.creditPoint} tín chỉ</small></Link>)}</div>
            </DashboardPanel>
            <DashboardPanel title="Kỳ thi & kết quả" link="/student/exam-results" linkText="Xem tất cả">
              {exams.length ? <div className="exam-compact-list">{exams.map(item => <Link to="/student/exam-results" key={item.examResultId}><Tag color="red">{dayjs(item.examStartDate).format('DD/MM/YYYY')}</Tag><span><b>{item.examName}</b><small>{item.description ?? 'Thông tin kỳ thi'}</small></span><RightOutlined /></Link>)}</div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chưa có kỳ thi hoặc kết quả" />}
            </DashboardPanel>
          </div>
        </div>
        <aside className="student-dashboard-aside">
          <DashboardPanel title="Thao tác nhanh"><div className="student-quick-actions">
            <QuickAction to="/student/subjects" icon={<BookOutlined />} label="Lớp học phần" tone="purple" />
            <QuickAction to="/student/schedule" icon={<CalendarOutlined />} label="Lịch học" tone="blue" />
            <QuickAction to="/student/attendance" icon={<CheckCircleOutlined />} label="Điểm danh" tone="green" />
            <QuickAction to="/student/exam-results" icon={<SolutionOutlined />} label="Thi & Kết quả" tone="orange" />
            <QuickAction to="/student/announcements" icon={<FileTextOutlined />} label="Thông báo" tone="gold" />
            <QuickAction to="/student/form-requests" icon={<FormOutlined />} label="Biểu mẫu" tone="purple" />
          </div></DashboardPanel>
          <Card className="student-help-card"><div className="help-illustration">?</div><Typography.Title level={4}>Cần hỗ trợ?</Typography.Title><Typography.Text>Liên hệ Trung tâm hỗ trợ sinh viên</Typography.Text><Link to="/student/form-requests">Liên hệ ngay</Link></Card>
        </aside>
      </div>
      <div className="student-profile-strip"><b>{profile.majorName}</b><span>{profile.facultyName ?? 'Chưa xác định khoa'}</span><span>Niên khóa {profile.academicYearName}</span><span>Trạng thái: {raw(profile.studyStatus)}</span></div>
      <Typography.Text className="student-data-note" type="secondary">Không hiển thị GPA, tín chỉ tích lũy, đậu/rớt hoặc tỷ lệ điểm danh khi chưa có quy tắc nghiệp vụ được xác nhận.</Typography.Text>
    </div>
  }}</QueryState>
}

function DashboardMetric({ icon, label, value, note, tone }: { icon: ReactNode; label: string; value: number; note: string; tone: string }) {
  return <Card className={`student-metric tone-${tone}`}><span>{icon}</span><Statistic title={label} value={value} /><small>{note}</small></Card>
}

function DashboardPanel({ title, link, linkText, children }: { title: string; link?: string; linkText?: string; children: ReactNode }) {
  return <Card className="student-dashboard-panel" title={title} extra={link && <Link to={link}>{linkText} <RightOutlined /></Link>}>{children}</Card>
}

function QuickAction({ to, icon, label, tone }: { to: string; icon: ReactNode; label: string; tone: string }) {
  return <Link to={to}><span className={`tone-${tone}`}>{icon}</span><b>{label}</b></Link>
}

export function StudentProfilePage() {
  const query = useQuery({ queryKey: ['student', 'profile'], queryFn: studentApi.profile })
  return <div className="student-profile-page">
    <div className="student-page-heading"><Typography.Title level={2}>Hồ sơ của tôi</Typography.Title><Typography.Text type="secondary">Trang chủ&nbsp;&nbsp;/&nbsp;&nbsp;Hồ sơ của tôi</Typography.Text></div>
    <QueryState query={query}>{(p) => <>
      <Card className="student-profile-hero">
        <div className="profile-identity">
          <Avatar size={130} src={p.profilePicUrl} icon={<UserOutlined />}>{p.fullName.slice(0, 1)}</Avatar>
          <div><Typography.Title level={2}>{p.fullName}</Typography.Title><Space wrap><Tag color="blue">{p.studentCode}</Tag><Tag color="green">{studyStatusLabel(p.studyStatus)}</Tag></Space>
            <Typography.Text type="secondary">Tên đăng nhập: {p.userName}</Typography.Text>
            <div className="profile-meta"><span><BankOutlined /> {p.facultyName ?? 'Chưa cập nhật khoa'}</span><span><ApartmentOutlined /> {p.majorName}</span><span><CalendarOutlined /> Khóa {p.academicYearName}</span></div>
          </div>
        </div>
        <div className="profile-statuses">
          <ProfileStatus icon={<ReadOutlined />} label="Trạng thái học" value={studyStatusLabel(p.studyStatus)} tone="green" />
          <ProfileStatus icon={<TrophyOutlined />} label="Trạng thái tốt nghiệp" value={p.isGraduated ? 'Đã tốt nghiệp' : 'Chưa tốt nghiệp'} tone="purple" />
          <ProfileStatus icon={<SafetyCertificateOutlined />} label="Trạng thái tài khoản" value="Hoạt động" tone="blue" />
        </div>
      </Card>
      <div className="student-profile-tabs" role="tablist" aria-label="Nhóm thông tin hồ sơ"><span className="active" role="tab" aria-selected="true"><ReadOutlined /> Học vụ</span><span role="tab" aria-selected="false"><UserOutlined /> Cá nhân</span><span role="tab" aria-selected="false"><IdcardOutlined /> Thông tin bổ sung</span></div>
      <div className="student-profile-grid">
        <ProfileInfoCard title="Thông tin học vụ" rows={[
          ['Mã sinh viên', p.studentCode], ['Khoa', p.facultyName ?? 'Chưa cập nhật'], ['Ngành', `${p.majorCode} · ${p.majorName}`],
          ['Khóa / Năm học', `${p.academicYearName} (${p.academicYear})`], ['Trạng thái học', studyStatusLabel(p.studyStatus)],
          ['Trạng thái tốt nghiệp', p.isGraduated ? 'Đã tốt nghiệp' : 'Chưa tốt nghiệp'],
        ]} />
        <ProfileInfoCard title="Thông tin tài khoản" rows={[
          ['Họ và tên', p.fullName], ['Tên đăng nhập', p.userName], ['Mã người dùng', maskIdentifier(p.userId)],
          ['Trạng thái tài khoản', 'Hoạt động'], ['Phiên đăng nhập hết hạn', dayjs(p.expiresAt).isValid() ? dayjs(p.expiresAt).format('DD/MM/YYYY HH:mm') : 'Chưa cập nhật'],
        ]} />
        <Card className="profile-info-card profile-status-card" title="Tình trạng hồ sơ">
          <div className={`profile-issue ${p.hasIssue ? 'has-issue' : ''}`}><span>{p.hasIssue ? <WarningOutlined /> : <CheckCircleOutlined />}</span><div><b>{p.hasIssue ? 'Hồ sơ có cờ cần lưu ý' : 'Hồ sơ chưa có cảnh báo'}</b><small>{p.hasIssue ? 'Vui lòng liên hệ Phòng Đào tạo để được kiểm tra.' : 'Thông tin hiện có không ghi nhận vấn đề.'}</small></div></div>
          <Alert type="info" showIcon message="Hồ sơ chỉ đọc" description="Nếu thông tin có sai sót, vui lòng gửi yêu cầu để Phòng Đào tạo hỗ trợ cập nhật." />
          <Link className="profile-support-link" to="/student/form-requests">Gửi yêu cầu hỗ trợ <RightOutlined /></Link>
        </Card>
      </div>
      <Typography.Text className="student-profile-note" type="secondary">Thông tin hiển thị được lấy từ hệ thống. Dữ liệu cá nhân và liên hệ chưa được cung cấp qua API sinh viên nên không hiển thị trên trang này.</Typography.Text>
    </>}</QueryState>
  </div>
}

function studyStatusLabel(status?: number | null) {
  return status == null ? 'Chưa cập nhật' : `Trạng thái ${status}`
}

function maskIdentifier(value: string) {
  return value.length < 10 ? value : `${value.slice(0, 8)}…${value.slice(-6)}`
}

function ProfileStatus({ icon, label, value, tone }: { icon: ReactNode; label: string; value: string; tone: string }) {
  return <div className="profile-status"><span className={`tone-${tone}`}>{icon}</span><small>{label}</small><b>{value}</b></div>
}

function ProfileInfoCard({ title, rows }: { title: string; rows: [string, string][] }) {
  return <Card className="profile-info-card" title={title}>{rows.map(([label, value]) => <div className="profile-info-row" key={label}><span>{label}</span><b>{value || 'Chưa cập nhật'}</b></div>)}</Card>
}

export function StudentProgramPage() {
  const query = useQuery({ queryKey: ['student', 'program'], queryFn: async () => {
    const [program, profile] = await Promise.all([studentApi.program(), studentApi.profile()])
    return { program, profile }
  } })
  return <div className="student-program-page">
    <div className="student-page-heading"><Typography.Title level={2}>Chương trình & kế hoạch học tập</Typography.Title><Typography.Text type="secondary">Trang chủ&nbsp;&nbsp;/&nbsp;&nbsp;Chương trình & kế hoạch học tập</Typography.Text></div>
    <QueryState query={query}>{({ program, profile }) => <ProgramView program={program} profile={profile} />}</QueryState>
  </div>
}

function ProgramView({ program, profile }: { program: import('./types').StudentProgram; profile: import('./types').StudentProfile }) {
  const plans = [...program.semesterPlans].sort((a, b) => a.semester - b.semester)
  const [selectedSemester, setSelectedSemester] = useState<number | null>(plans[0]?.semester ?? null)
  const [expandedSemester, setExpandedSemester] = useState<number | null>(plans[0]?.semester ?? null)
  const [search, setSearch] = useState('')
  const totalCredits = plans.reduce((sum, plan) => sum + plan.subjects.reduce((value, subject) => value + subject.creditPoint, 0), 0)
  const totalSubjects = plans.reduce((sum, plan) => sum + plan.subjects.length, 0)
  const normalizedSearch = search.trim().toLocaleLowerCase('vi')

  return <>
    <section className="program-summary">
      <Card className="program-major-card"><div className="program-major-icon"><ReadOutlined /></div><div><small>Ngành</small><b>{program.majorName}</b><span>Mã ngành: {program.majorCode}</span></div><div><small>Khóa / Năm học</small><b>{program.academicYearName}</b><span>Năm: {profile.academicYear}</span></div><div><small>Khoa</small><b>{profile.facultyName ?? 'Chưa cập nhật'}</b><span>{profile.facultyCode ?? 'Chưa có mã khoa'}</span></div></Card>
      <Card className="program-totals">
        <ProgramTotal icon={<BookOutlined />} value={plans.length} label="Học kỳ" tone="blue" />
        <ProgramTotal icon={<TrophyOutlined />} value={totalCredits} label="Tổng tín chỉ" tone="green" />
        <ProgramTotal icon={<FileTextOutlined />} value={totalSubjects} label="Môn học" tone="purple" />
      </Card>
    </section>
    <div className="program-toolbar">
      <div><Typography.Text>Chọn học kỳ</Typography.Text><div className="semester-selector">{plans.map(plan => <button className={selectedSemester === plan.semester ? 'active' : ''} key={plan.id} onClick={() => { setSelectedSemester(plan.semester); setExpandedSemester(plan.semester) }}><b>HK {plan.semester}</b><span>{dayjs(plan.startDate).format('YYYY')} – {dayjs(plan.endDate).format('YYYY')}</span></button>)}</div></div>
      <Input allowClear prefix={<SearchOutlined />} value={search} onChange={event => setSearch(event.target.value)} placeholder="Tìm kiếm mã môn hoặc tên môn..." />
    </div>
    <div className="program-content-layout">
      <main className="semester-plans">
        {plans.length === 0 ? <Card><Empty description="Chưa có kế hoạch học kỳ" /></Card> : plans.map(plan => {
          const isOpen = expandedSemester === plan.semester
          const subjects = plan.subjects.filter(subject => !normalizedSearch || `${subject.subjectCode} ${subject.subjectName}`.toLocaleLowerCase('vi').includes(normalizedSearch))
          const credits = plan.subjects.reduce((sum, subject) => sum + subject.creditPoint, 0)
          return <section className={`semester-plan ${isOpen ? 'open' : ''}`} key={plan.id}>
            <button className="semester-plan-heading" onClick={() => setExpandedSemester(isOpen ? null : plan.semester)} aria-expanded={isOpen}>
              <span><b>Học kỳ {plan.semester} ({dayjs(plan.startDate).format('YYYY')} – {dayjs(plan.endDate).format('YYYY')})</b>{plan.isActive && <Tag color="green">Đang áp dụng</Tag>}</span>
              <span>Thời gian: {dayjs(plan.startDate).format('DD/MM/YYYY')} – {dayjs(plan.endDate).format('DD/MM/YYYY')} <i>·</i> Số môn: {plan.subjects.length} <i>·</i> Tổng tín chỉ: {credits}</span>
              {isOpen ? <UpOutlined /> : <DownOutlined />}
            </button>
            {isOpen && <div className="semester-plan-body">
              <div className="program-subject-table">
                <div className="program-subject-head"><b>STT</b><b>Mã môn</b><b>Tên môn</b><b>Số tín chỉ</b><b>Liên kết</b></div>
                {subjects.length ? subjects.map((subject, index) => <div className="program-subject-row" key={subject.id}><span>{index + 1}</span><b>{subject.subjectCode}</b><span>{subject.subjectName}</span><span>{subject.creditPoint}</span><Link to="/student/subjects">Xem môn <RightOutlined /></Link></div>) : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không tìm thấy môn phù hợp" />}
              </div>
              <div className="program-snapshot-warning"><WarningOutlined /> Môn học hiển thị theo mã và tên tại thời điểm lập kế hoạch; dữ liệu có thể khác danh mục môn hiện tại.</div>
            </div>}
          </section>
        })}
      </main>
      <aside className="program-aside">
        <Card title="Ghi chú"><div className="program-notes"><p><InfoCircleOutlined /> Đây là kế hoạch học tập tham chiếu của ngành/khóa bạn đang theo học.</p><p><WarningOutlined /> Chưa có dữ liệu để xác định môn bắt buộc, tự chọn hoặc tổng số giờ.</p><p><CheckCircleOutlined /> Liên kết tới danh sách lớp học phần khi có dữ liệu liên quan.</p></div></Card>
        <Card title="Thông tin sinh viên"><ProfileInfoCardRows rows={[['Mã sinh viên', profile.studentCode], ['Ngành', profile.majorName], ['Trạng thái học', studyStatusLabel(profile.studyStatus)], ['Tốt nghiệp', profile.isGraduated ? 'Đã tốt nghiệp' : 'Chưa tốt nghiệp']]} /></Card>
      </aside>
    </div>
  </>
}

function ProgramTotal({ icon, value, label, tone }: { icon: ReactNode; value: number; label: string; tone: string }) {
  return <div className="program-total"><span className={`tone-${tone}`}>{icon}</span><div><b>{value}</b><small>{label}<em>(Kế hoạch)</em></small></div></div>
}

function ProfileInfoCardRows({ rows }: { rows: [string, string][] }) {
  return <>{rows.map(([label, value]) => <div className="profile-info-row" key={label}><span>{label}</span><b>{value}</b></div>)}</>
}

export function StudentSubjectsPage() {
  const query = useQuery({ queryKey: ['student', 'subjects'], queryFn: async () => {
    const [subjects, schedule] = await Promise.all([studentApi.subjects(), studentApi.schedule()])
    return { subjects, schedule }
  } })
  return <div className="student-courses-page">
    <div className="student-page-heading"><Typography.Title level={2}>Môn và lớp học phần của tôi</Typography.Title><Typography.Text type="secondary">Danh sách các lớp học phần bạn đã được ghi danh.</Typography.Text></div>
    <QueryState query={query}>{data => <StudentCoursesView {...data} />}</QueryState>
  </div>
}

type CourseStatus = 'all' | 'upcoming' | 'ongoing' | 'ended'

function StudentCoursesView({ subjects, schedule }: { subjects: StudentSubject[]; schedule: ScheduleItem[] }) {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<CourseStatus>('all')
  const [from, setFrom] = useState<string>()
  const [to, setTo] = useState<string>()
  const [selectedId, setSelectedId] = useState<string | null>(subjects[0]?.enrollmentId ?? null)
  const normalized = search.trim().toLocaleLowerCase('vi')
  const courseStatus = (item: StudentSubject): Exclude<CourseStatus, 'all'> => dayjs().isBefore(dayjs(item.startDate), 'day') ? 'upcoming' : dayjs().isAfter(dayjs(item.endDate), 'day') ? 'ended' : 'ongoing'
  const counts = { upcoming: 0, ongoing: 0, ended: 0 }
  subjects.forEach(item => { counts[courseStatus(item)]++ })
  const filtered = subjects.filter(item => {
    const matchesSearch = !normalized || `${item.subjectCode} ${item.subjectName} ${item.className}`.toLocaleLowerCase('vi').includes(normalized)
    const matchesStatus = status === 'all' || courseStatus(item) === status
    const matchesDates = (!from || !dayjs(item.endDate).isBefore(dayjs(from), 'day')) && (!to || !dayjs(item.startDate).isAfter(dayjs(to), 'day'))
    return matchesSearch && matchesStatus && matchesDates
  })
  const selected = subjects.find(item => item.enrollmentId === selectedId) ?? null
  const selectedSchedules = selected ? schedule.filter(item => item.subjectTeachingId === selected.subjectTeachingId) : []
  const teachers = [...new Set(selectedSchedules.map(item => item.teacherName).filter((name): name is string => Boolean(name)))]

  return <div className="courses-layout">
    <main className="courses-list-panel">
      <Card className="courses-list-card">
        <div className="courses-filters"><Input allowClear prefix={<SearchOutlined />} placeholder="Tìm kiếm mã môn, tên môn hoặc tên lớp..." value={search} onChange={event => setSearch(event.target.value)} /><DatePicker.RangePicker onChange={dates => { setFrom(dates?.[0]?.format('YYYY-MM-DD')); setTo(dates?.[1]?.format('YYYY-MM-DD')) }} /></div>
        <div className="course-status-filters">
          <button className={status === 'all' ? 'active' : ''} onClick={() => setStatus('all')}>Tất cả ({subjects.length})</button>
          <button className={status === 'upcoming' ? 'active' : ''} onClick={() => setStatus('upcoming')}><i className="dot orange" /> Sắp diễn ra ({counts.upcoming})</button>
          <button className={status === 'ongoing' ? 'active' : ''} onClick={() => setStatus('ongoing')}><i className="dot green" /> Đang diễn ra ({counts.ongoing})</button>
          <button className={status === 'ended' ? 'active' : ''} onClick={() => setStatus('ended')}><i className="dot gray" /> Đã kết thúc ({counts.ended})</button>
        </div>
        <div className="courses-table">
          <div className="courses-table-head"><b>Môn học</b><b>Lớp học phần</b><b>Tín chỉ</b><b>Thời gian</b><b>Phòng mặc định</b><b>Trạng thái</b><span /></div>
          {filtered.length ? filtered.map(item => <button className={`course-row ${selectedId === item.enrollmentId ? 'selected' : ''}`} key={item.enrollmentId} onClick={() => setSelectedId(item.enrollmentId)}>
            <span><b>{item.subjectCode}</b><small>{item.subjectName}</small></span><span><b>{item.className}</b><small>{item.subjectTeachingId.slice(0, 8)}</small></span><span>{item.creditPoint}</span><span>{dayjs(item.startDate).format('DD/MM/YYYY')}<small>– {dayjs(item.endDate).format('DD/MM/YYYY')}</small></span><span>{item.defaultRoom ?? 'Chưa cập nhật'}</span><CourseStatusTag status={courseStatus(item)} /><RightOutlined />
          </button>) : <Empty description="Không tìm thấy lớp học phần phù hợp" />}
        </div>
        <div className="courses-count">Hiển thị {filtered.length} trong tổng số {subjects.length} lớp học phần</div>
      </Card>
      <Alert className="courses-note" type="info" showIcon message="Lưu ý" description="Trạng thái lớp được xác định trung tính theo ngày bắt đầu và kết thúc. Chọn từng dòng để xem chi tiết." />
    </main>
    <aside className="course-detail-panel">
      {selected ? <CourseDetail item={selected} status={courseStatus(selected)} schedules={selectedSchedules} teachers={teachers} /> : <Card><Empty description="Chọn một lớp học phần để xem chi tiết" /></Card>}
    </aside>
  </div>
}

function CourseStatusTag({ status }: { status: Exclude<CourseStatus, 'all'> }) {
  const map = { upcoming: ['orange', 'Sắp diễn ra'], ongoing: ['green', 'Đang diễn ra'], ended: ['default', 'Đã kết thúc'] } as const
  return <Tag color={map[status][0]}>{map[status][1]}</Tag>
}

function CourseDetail({ item, status, schedules, teachers }: { item: StudentSubject; status: Exclude<CourseStatus, 'all'>; schedules: ScheduleItem[]; teachers: string[] }) {
  return <Card className="course-detail-card" title="Chi tiết lớp học phần">
    <div className="course-detail-title"><span><BookOutlined /></span><div><b>{item.subjectCode} - {item.subjectName}</b><small>{item.className}</small><CourseStatusTag status={status} /></div></div>
    <div className="course-detail-tabs"><span className="active">Tổng quan</span><span>Giảng viên</span><span>Lịch học</span><span>Tài liệu</span></div>
    <section className="course-detail-section"><ProfileInfoCardRows rows={[['Mã môn', item.subjectCode], ['Tên môn', item.subjectName], ['Tên lớp học phần', item.className], ['Số tín chỉ', String(item.creditPoint)], ['Tổng số buổi', String(item.totalSessions)], ['Ngày bắt đầu', dayjs(item.startDate).format('DD/MM/YYYY')], ['Ngày kết thúc', dayjs(item.endDate).format('DD/MM/YYYY')], ['Phòng mặc định', item.defaultRoom ?? 'Chưa cập nhật']]} /></section>
    <section className="course-detail-section"><h4>Giảng viên</h4>{teachers.length ? teachers.map(name => <div className="course-teacher" key={name}><Avatar icon={<UserOutlined />} /><span><small>Giảng viên</small><b>{name}</b></span></div>) : <Typography.Text type="secondary">Chưa cập nhật giảng viên</Typography.Text>}</section>
    <section className="course-detail-section"><h4>Lịch liên quan</h4><Typography.Text>{schedules.length} buổi học hiện có trong thời khóa biểu.</Typography.Text></section>
    <section className="course-quick-links"><h4>Thao tác nhanh</h4><div><Link to="/student/schedule"><CalendarOutlined />Lịch học</Link><Link to="/student/documents"><FileTextOutlined />Tài liệu</Link><Link to="/student/attendance"><CheckCircleOutlined />Điểm danh</Link><Link to="/student/exam-results"><TrophyOutlined />Kết quả</Link></div></section>
  </Card>
}

export function StudentSchedulePage() {
  const query = useQuery({ queryKey: ['student', 'schedule'], queryFn: studentApi.schedule })
  return <div className="student-timetable-page"><div className="student-page-heading"><Typography.Title level={2}>Thời khóa biểu</Typography.Title><Typography.Text type="secondary">Xem lịch học cá nhân theo tuần hoặc danh sách.</Typography.Text></div><QueryState query={query}>{items => <TimetableView items={items} />}</QueryState></div>
}

const mondayOf = (date: dayjs.Dayjs) => date.subtract(date.day() === 0 ? 6 : date.day() - 1, 'day').startOf('day')

function TimetableView({ items }: { items: ScheduleItem[] }) {
  const initial = items.length ? dayjs(items[0].startDateTime) : dayjs()
  const [weekStart, setWeekStart] = useState(mondayOf(initial))
  const [view, setView] = useState<'week' | 'list'>('week')
  const [mobileDay, setMobileDay] = useState<number>(initial.day())
  const days = Array.from({ length: 7 }, (_, index) => weekStart.add(index, 'day'))
  const weekItems = items.filter(item => {
    const date = dayjs(item.startDateTime)
    return !date.isBefore(weekStart, 'day') && !date.isAfter(weekStart.add(6, 'day'), 'day')
  })
  const subjectColors = new Map<string, number>()
  items.forEach(item => { if (!subjectColors.has(item.subjectCode)) subjectColors.set(item.subjectCode, subjectColors.size % 6) })
  const upcoming = [...items].filter(item => dayjs(item.endDateTime).isAfter(dayjs())).sort((a, b) => dayjs(a.startDateTime).valueOf() - dayjs(b.startDateTime).valueOf()).slice(0, 5)
  const mobileItems = weekItems.filter(item => dayjs(item.startDateTime).day() === mobileDay)

  return <>
    <Card className="timetable-toolbar"><div>
      <Button icon={<LeftOutlined />} onClick={() => setWeekStart(value => value.subtract(7, 'day'))}>Tuần trước</Button>
      <Button icon={<CalendarOutlined />} onClick={() => setWeekStart(mondayOf(dayjs()))}>Hôm nay</Button>
      <Button onClick={() => setWeekStart(value => value.add(7, 'day'))}>Tuần sau <RightOutlined /></Button>
    </div><Button className="week-range-button" icon={<CalendarOutlined />}>{weekStart.format('DD/MM/YYYY')} - {weekStart.add(6, 'day').format('DD/MM/YYYY')}</Button>
      <Segmented value={view} onChange={value => setView(value as 'week' | 'list')} options={[{ value: 'week', label: 'Tuần' }, { value: 'list', label: 'Danh sách' }]} />
      <Button icon={<FilterOutlined />}>Bộ lọc</Button>
    </Card>
    <div className="timetable-layout">
      <main>
        {view === 'week' ? <div className="schedule-desktop timetable-grid">
          <div className="timetable-header"><b>Giờ</b>{days.map((date, index) => <div key={date.format('YYYY-MM-DD')}><b>{index === 6 ? 'Chủ nhật' : `Thứ ${index + 2}`}</b><span>{date.format('DD/MM')}</span></div>)}</div>
          <div className="timetable-body"><div className="time-axis">{Array.from({ length: 13 }, (_, index) => <span key={index}>{String(index + 7).padStart(2, '0')}:00</span>)}</div>{days.map(date => <div className="timetable-day-column" key={date.format('YYYY-MM-DD')}>{weekItems.filter(item => dayjs(item.startDateTime).isSame(date, 'day')).map(item => <TimetableEvent key={item.scheduleId} item={item} color={subjectColors.get(item.subjectCode) ?? 0} />)}</div>)}</div>
        </div> : <Card className="timetable-list-view">{weekItems.length ? weekItems.map(item => <div className="timetable-list-row" key={item.scheduleId}><time>{dayjs(item.startDateTime).format('ddd, DD/MM')}<b>{dayjs(item.startDateTime).format('HH:mm')} – {dayjs(item.endDateTime).format('HH:mm')}</b></time><span><b>{item.subjectCode} · {item.subjectName}</b><small>{item.className}</small></span><span>{item.roomName ?? 'Chưa có phòng'}<small>{item.teacherName ?? 'Chưa có giảng viên'}</small></span></div>) : <Empty description="Tuần này chưa có lịch học" />}</Card>}
        <div className="schedule-mobile"><Segmented block value={mobileDay} onChange={value => setMobileDay(Number(value))} options={days.map(date => ({ value: date.day(), label: date.day() === 0 ? 'CN' : `T${date.day() + 1}` }))} /><div className="mobile-schedule-list">{mobileItems.length ? mobileItems.map(item => <div key={item.scheduleId}><Tag color="blue">{dayjs(item.startDateTime).format('HH:mm')} – {dayjs(item.endDateTime).format('HH:mm')}</Tag><b>{item.subjectCode} · {item.subjectName}</b><span>{item.roomName ?? 'Chưa có phòng'} · {item.teacherName ?? 'Chưa có giảng viên'}</span></div>) : <Empty description="Không có lịch trong ngày" />}</div></div>
        <div className="timetable-legend">{[...subjectColors.entries()].map(([code, color]) => { const item = items.find(value => value.subjectCode === code)!; return <span key={code}><i className={`event-color-${color}`} />{code} {item.subjectName}</span> })}</div>
        <Alert className="timetable-note" type="info" showIcon message="Ghi chú" description="Thời khóa biểu có thể thay đổi theo thông báo của Khoa/Phòng Đào tạo. Vui lòng kiểm tra lịch thường xuyên để cập nhật thông tin mới nhất." />
      </main>
      <aside className="timetable-aside">
        <Card title="Chọn ngày"><AntCalendar fullscreen={false} value={weekStart} onSelect={date => { setWeekStart(mondayOf(date)); setMobileDay(date.day()) }} /></Card>
        <Card title="Lịch sắp tới" extra={<Link to="/student/schedule">Xem tất cả</Link>}><div className="upcoming-timetable">{upcoming.length ? upcoming.map(item => <div key={item.scheduleId} className={`upcoming-color-${subjectColors.get(item.subjectCode) ?? 0}`}><b>{item.subjectCode} - {item.subjectName}</b><span>{dayjs(item.startDateTime).format('DD/MM · HH:mm')} – {dayjs(item.endDateTime).format('HH:mm')}</span><small>{item.roomName ?? 'Chưa có phòng'} · {item.teacherName ?? 'Chưa có giảng viên'}</small></div>) : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chưa có lịch sắp tới" />}</div></Card>
        <Button className="timetable-list-link" icon={<UnorderedListOutlined />} onClick={() => setView('list')}>Xem thời khóa biểu dạng danh sách <RightOutlined /></Button>
      </aside>
    </div>
  </>
}

function TimetableEvent({ item, color }: { item: ScheduleItem; color: number }) {
  const start = dayjs(item.startDateTime)
  const end = dayjs(item.endDateTime)
  const startMinutes = start.hour() * 60 + start.minute()
  const endMinutes = end.hour() * 60 + end.minute()
  const top = Math.max(0, (startMinutes - 7 * 60) / 60 * 56)
  const height = Math.max(44, (endMinutes - startMinutes) / 60 * 56)
  return <div className={`timetable-event event-color-${color}`} style={{ top, height }}><b>{item.subjectCode}</b><span>{item.subjectName}</span><span>{start.format('HH:mm')} - {end.format('HH:mm')}</span><small>{item.roomName ?? 'Chưa có phòng'}</small><small>{item.teacherName ?? 'Chưa có giảng viên'}</small></div>
}

export function StudentExamResultsPage() {
  const query = useQuery({ queryKey: ['student', 'exam-results'], queryFn: async () => {
    const [exams, subjects, schedule] = await Promise.all([studentApi.examResults(), studentApi.subjects(), studentApi.schedule()])
    return { exams, subjects, schedule }
  } })
  return <div className="student-exams-page"><div className="student-page-heading"><Typography.Title level={2}>Kỳ thi và kết quả</Typography.Title><Typography.Text type="secondary">Trang chủ&nbsp;&nbsp;/&nbsp;&nbsp;Kỳ thi và kết quả</Typography.Text></div><QueryState query={query}>{data => <ExamResultsView {...data} />}</QueryState></div>
}

type ExamTimeStatus = 'all' | 'upcoming' | 'ongoing' | 'ended'

function ExamResultsView({ exams, subjects, schedule }: { exams: ExamResult[]; subjects: StudentSubject[]; schedule: ScheduleItem[] }) {
  const [tab, setTab] = useState<'exams' | 'results'>('exams')
  const [subjectId, setSubjectId] = useState<string>()
  const [status, setStatus] = useState<ExamTimeStatus>('all')
  const [from, setFrom] = useState<string>()
  const [to, setTo] = useState<string>()
  const [selectedId, setSelectedId] = useState<string | null>(exams[0]?.examResultId ?? null)
  const subjectByTeaching = new Map(subjects.map(item => [item.subjectTeachingId, item]))
  const examStatus = (item: ExamResult): Exclude<ExamTimeStatus, 'all'> => dayjs().isBefore(dayjs(item.examStartDate)) ? 'upcoming' : dayjs().isAfter(dayjs(item.examEndDate)) ? 'ended' : 'ongoing'
  const filtered = exams.filter(item => (!subjectId || item.subjectTeachingId === subjectId) && (status === 'all' || examStatus(item) === status) && (!from || !dayjs(item.examEndDate).isBefore(dayjs(from), 'day')) && (!to || !dayjs(item.examStartDate).isAfter(dayjs(to), 'day')))
  const selected = exams.find(item => item.examResultId === selectedId) ?? null
  const selectedSubject = selected ? subjectByTeaching.get(selected.subjectTeachingId) : undefined
  const relatedSchedule = selected ? schedule.find(item => item.subjectTeachingId === selected.subjectTeachingId) : undefined
  const clear = () => { setSubjectId(undefined); setStatus('all'); setFrom(undefined); setTo(undefined) }

  return <div className="exams-layout">
    <main className="exams-main">
      <Card className="exam-filter-card">
        <div className="exam-tabs"><button className={tab === 'exams' ? 'active' : ''} onClick={() => setTab('exams')}>Kỳ thi</button><button className={tab === 'results' ? 'active' : ''} onClick={() => setTab('results')}>Kết quả</button></div>
        <div className="exam-filters"><label><span>Môn hoặc tên lớp học phần</span><Select allowClear showSearch value={subjectId} onChange={setSubjectId} placeholder="Tìm kiếm..." options={subjects.map(item => ({ value: item.subjectTeachingId, label: `${item.subjectCode} · ${item.subjectName} · ${item.className}` }))} /></label><label><span>Khoảng thời gian</span><DatePicker.RangePicker value={from && to ? [dayjs(from), dayjs(to)] : null} onChange={dates => { setFrom(dates?.[0]?.format('YYYY-MM-DD')); setTo(dates?.[1]?.format('YYYY-MM-DD')) }} /></label><label><span>Trạng thái</span><Select value={status} onChange={setStatus} options={[{ value: 'all', label: 'Tất cả' }, { value: 'upcoming', label: 'Sắp diễn ra' }, { value: 'ongoing', label: 'Đang diễn ra' }, { value: 'ended', label: 'Đã diễn ra' }]} /></label><Button onClick={clear}>Xóa bộ lọc</Button><Button type="primary" icon={<SearchOutlined />}>Tìm kiếm</Button></div>
      </Card>
      {tab === 'exams' ? <Card className="exam-list-card" title={<>Danh sách kỳ thi <Tag color="blue">Tất cả {filtered.length}</Tag></>}>
        <div className="exam-table-head"><b>Kỳ thi</b><b>Môn / Lớp học phần</b><b>Loại thi</b><b>Thời gian</b><b>Phòng thi</b><b>Giảng viên</b><b>Trạng thái</b><span /></div>
        {filtered.length ? filtered.map(item => {
          const course = subjectByTeaching.get(item.subjectTeachingId)
          const related = schedule.find(value => value.subjectTeachingId === item.subjectTeachingId)
          return <button className={`exam-row ${selectedId === item.examResultId ? 'selected' : ''}`} key={item.examResultId} onClick={() => setSelectedId(item.examResultId)}><span><b>{item.examName}</b><small>{item.subjectTeachingExamId.slice(0, 10)}</small></span><span><b>{course ? `${course.subjectCode} · ${course.subjectName}` : 'Chưa resolve môn'}</b><small>{course?.className ?? item.subjectTeachingId.slice(0, 8)}</small></span><span>Loại {item.examType}</span><span>{dayjs(item.examStartDate).format('DD/MM/YYYY')}<small>{dayjs(item.examStartDate).format('HH:mm')} – {dayjs(item.examEndDate).format('HH:mm')}</small></span><span>{related?.roomName ?? 'Chưa cập nhật'}</span><span>{related?.teacherName ?? 'Chưa cập nhật'}</span><ExamStatusTag status={examStatus(item)} /><RightOutlined /></button>
        }) : <Empty description="Không có kỳ thi phù hợp" />}
      </Card> : <ExamResultTable items={filtered} subjectByTeaching={subjectByTeaching} onSelect={setSelectedId} />}
      <ExamResultTable items={exams.slice(0, 5)} subjectByTeaching={subjectByTeaching} title="Kết quả gần đây" onSelect={setSelectedId} />
    </main>
    <aside className="exam-detail-aside">{selected ? <ExamDetail item={selected} subject={selectedSubject} schedule={relatedSchedule} status={examStatus(selected)} /> : <Card><Empty description="Chọn kỳ thi để xem chi tiết" /></Card>}</aside>
  </div>
}

function ExamStatusTag({ status }: { status: Exclude<ExamTimeStatus, 'all'> }) {
  const labels = { upcoming: ['green', 'Sắp diễn ra'], ongoing: ['orange', 'Đang diễn ra'], ended: ['default', 'Đã diễn ra'] } as const
  return <Tag color={labels[status][0]}>{labels[status][1]}</Tag>
}

function ExamResultTable({ items, subjectByTeaching, title, onSelect }: { items: ExamResult[]; subjectByTeaching: Map<string, StudentSubject>; title?: string; onSelect: (id: string) => void }) {
  return <Card className="exam-results-card" title={<>{title ?? 'Kết quả'} <Tag color="blue">Tất cả {items.length}</Tag></>}><div className="result-table-head"><b>Môn / Lớp học phần</b><b>Kỳ thi</b><b>Ngày nộp</b><b>Result</b><b>CombinedResult</b><b>Mô tả</b></div>{items.length ? items.map(item => { const course = subjectByTeaching.get(item.subjectTeachingId); return <button className="result-row" key={item.examResultId} onClick={() => onSelect(item.examResultId)}><span><b>{course ? `${course.subjectCode} · ${course.subjectName}` : 'Chưa resolve môn'}</b><small>{course?.className ?? item.subjectTeachingId.slice(0, 8)}</small></span><span>{item.examName}</span><span>{item.attemptSubmitDate ? dateTime(item.attemptSubmitDate) : 'Chưa nộp'}</span><strong>{raw(item.rawResult)}</strong><strong>{raw(item.rawCombinedResult)}</strong><span>{item.description ?? 'Giá trị ghi nhận'}</span></button> }) : <Empty description="Chưa có kết quả" />}</Card>
}

function ExamDetail({ item, subject, schedule, status }: { item: ExamResult; subject?: StudentSubject; schedule?: ScheduleItem; status: Exclude<ExamTimeStatus, 'all'> }) {
  return <Card className="exam-detail-card" title="Chi tiết kỳ thi"><ExamStatusTag status={status} /><div className="exam-detail-title"><span><BookOutlined /></span><div><b>{item.examName} - {subject?.subjectCode ?? 'Chưa xác định'}</b><small>{subject?.subjectName ?? 'Chưa resolve tên môn'}</small><small>{subject?.className ?? 'Chưa resolve lớp học phần'}</small></div></div><div className="exam-detail-tabs"><span className="active">Thông tin</span><span>Thành phần điểm</span><span>Ghi chú</span></div><section className="exam-detail-section"><ProfileInfoCardRows rows={[['Loại thi', `Loại ${item.examType}`], ['Phương thức thi', 'Chưa cập nhật'], ['Thời gian', `${dayjs(item.examStartDate).format('DD/MM/YYYY HH:mm')} – ${dayjs(item.examEndDate).format('HH:mm')}`], ['Phòng thi', schedule?.roomName ?? 'Chưa cập nhật'], ['Giảng viên', schedule?.teacherName ?? 'Chưa cập nhật'], ['Ghi chú', item.notes ?? '—']]} /></section><section className="exam-detail-section"><h4>Thông tin môn học</h4><ProfileInfoCardRows rows={[['Mã môn', subject?.subjectCode ?? 'Chưa cập nhật'], ['Tên môn', subject?.subjectName ?? 'Chưa cập nhật'], ['Lớp học phần', subject?.className ?? 'Chưa cập nhật'], ['Số tín chỉ', subject ? String(subject.creditPoint) : 'Chưa cập nhật']]} /></section><section className="exam-quick-links"><Link to="/student/subjects"><BookOutlined />Lớp học phần</Link><Link to="/student/schedule"><CalendarOutlined />Thời khóa biểu</Link><Link to="/student/documents"><FileTextOutlined />Tài liệu môn</Link></section><Alert type="info" showIcon message="Điểm là giá trị ghi nhận tạm thời. Không suy diễn đậu/rớt, điểm chữ hoặc điểm chính thức." /></Card>
}

export function StudentAttendancePage() {
  const query = useQuery({ queryKey: ['student', 'attendance'], queryFn: async () => {
    const [attendance, schedule] = await Promise.all([studentApi.attendance(), studentApi.schedule()])
    return { attendance, schedule }
  } })
  return <div className="student-attendance-page"><div className="student-page-heading"><Typography.Title level={2}>Điểm danh</Typography.Title><Typography.Text type="secondary">Xem bản ghi điểm danh của bạn theo từng buổi học.</Typography.Text></div><QueryState query={query}>{data => <AttendanceView {...data} />}</QueryState></div>
}

function AttendanceView({ attendance, schedule }: { attendance: Attendance[]; schedule: ScheduleItem[] }) {
  const [subject, setSubject] = useState<string>()
  const [status, setStatus] = useState<number>()
  const [from, setFrom] = useState<string>()
  const [to, setTo] = useState<string>()
  const [selectedId, setSelectedId] = useState<string | null>(attendance[0]?.attendanceId ?? null)
  const subjectOptions = [...new Map(attendance.map(item => [item.subjectCode, { value: item.subjectCode, label: `${item.subjectCode} · ${item.subjectName}` }])).values()]
  const statuses = [...new Set(attendance.map(item => item.rawStatus))].sort((a, b) => a - b)
  const filtered = attendance.filter(item => (!subject || item.subjectCode === subject) && (status == null || item.rawStatus === status) && (!from || !dayjs(item.startDateTime).isBefore(dayjs(from), 'day')) && (!to || !dayjs(item.startDateTime).isAfter(dayjs(to), 'day')))
  const selected = attendance.find(item => item.attendanceId === selectedId) ?? null
  const selectedSchedule = selected ? schedule.find(item => item.scheduleId === selected.scheduleId) : undefined
  const clear = () => { setSubject(undefined); setStatus(undefined); setFrom(undefined); setTo(undefined) }

  return <>
    <Card className="attendance-filter-card"><div className="attendance-filters">
      <label><span>Môn / Lớp học phần</span><Select allowClear showSearch value={subject} onChange={setSubject} options={subjectOptions} placeholder="Tìm môn hoặc lớp học phần..." /></label>
      <label><span>Khoảng ngày</span><DatePicker.RangePicker value={from && to ? [dayjs(from), dayjs(to)] : null} onChange={dates => { setFrom(dates?.[0]?.format('YYYY-MM-DD')); setTo(dates?.[1]?.format('YYYY-MM-DD')) }} /></label>
      <label><span>Trạng thái</span><Select allowClear value={status} onChange={setStatus} options={statuses.map(value => ({ value, label: `Trạng thái ${value}` }))} placeholder="Tất cả trạng thái" /></label>
      <Button onClick={clear}>Xóa bộ lọc</Button><Button type="primary" icon={<SearchOutlined />}>Tìm kiếm</Button>
    </div></Card>
    <div className="attendance-layout">
      <main>
        <Card className="attendance-summary"><AttendanceMetric icon={<FileTextOutlined />} label="Tổng bản ghi" value={attendance.length} tone="blue" />{statuses.slice(0, 4).map((value, index) => <AttendanceMetric key={value} icon={[<CheckCircleOutlined />, <ClockCircleOutlined />, <UserOutlined />, <CloseCircleOutlined />][index]} label={`Trạng thái ${value}`} value={attendance.filter(item => item.rawStatus === value).length} tone={['green', 'gold', 'orange', 'red'][index]} />)}</Card>
        <Card className="attendance-table-card">
          <div className="attendance-table-head"><b>Ngày</b><b>Môn học / Lớp học phần</b><b>Phòng</b><b>Trạng thái</b><b>Ghi chú</b><b>Ghi nhận lúc</b><span /></div>
          {filtered.length ? filtered.map(item => {
            const related = schedule.find(value => value.scheduleId === item.scheduleId)
            return <button className={`attendance-row ${selectedId === item.attendanceId ? 'selected' : ''}`} key={item.attendanceId} onClick={() => setSelectedId(item.attendanceId)}>
              <span><b>{dayjs(item.startDateTime).format('dddd')}</b>{dayjs(item.startDateTime).format('DD/MM/YYYY')}<small>{dayjs(item.startDateTime).format('HH:mm')} – {dayjs(item.endDateTime).format('HH:mm')}</small></span>
              <span><b>{item.subjectCode} · {item.subjectName}</b><small>{item.className}</small></span><span>{related?.roomName ?? 'Chưa cập nhật'}</span><RawStatusTag value={item.rawStatus} /><span>{item.notes || '—'}</span><span>{dayjs(item.creationDate).format('DD/MM/YYYY')}<small>{dayjs(item.creationDate).format('HH:mm')}</small></span><RightOutlined />
            </button>
          }) : <Empty description="Không có bản ghi phù hợp" />}
          <div className="attendance-table-footer">Hiển thị {filtered.length} trong tổng số {attendance.length} bản ghi</div>
        </Card>
      </main>
      <aside className="attendance-aside">
        <Alert type="info" showIcon message="Lưu ý" description="Ý nghĩa các trạng thái điểm danh đang được nhà trường xác nhận." />
        {selected ? <Card className="attendance-detail-card" title="Chi tiết buổi học">
          <RawStatusTag value={selected.rawStatus} />
          <div className="attendance-detail-title"><span><BookOutlined /></span><div><b>{selected.subjectCode} · {selected.subjectName}</b><small>{selected.className}</small></div></div>
          <ProfileInfoCardRows rows={[['Thời gian', `${dayjs(selected.startDateTime).format('DD/MM/YYYY HH:mm')} – ${dayjs(selected.endDateTime).format('HH:mm')}`], ['Phòng học', selectedSchedule?.roomName ?? 'Chưa cập nhật'], ['Giảng viên', selectedSchedule?.teacherName ?? 'Chưa cập nhật'], ['Loại lịch', selectedSchedule?.scheduleType == null ? 'Chưa cập nhật' : String(selectedSchedule.scheduleType)], ['Ghi chú', selected.notes || '—'], ['Ghi nhận lúc', dateTime(selected.creationDate)], ['Cờ cảnh báo', selected.isFirstTypeWarning || selected.isSecondTypeWarning ? 'Có' : 'Không']]} />
          <section className="attendance-quick-links"><h4>Liên kết nhanh</h4><Link to="/student/subjects">Xem lớp học phần <RightOutlined /></Link><Link to="/student/schedule">Xem thời khóa biểu <RightOutlined /></Link><Link to="/student/exam-results">Xem kết quả học tập <RightOutlined /></Link></section>
          <Alert type="info" showIcon message="Nhãn trạng thái đang được xác nhận; hệ thống không tự suy diễn có mặt, đi trễ hoặc vắng." />
        </Card> : <Card><Empty description="Chọn một bản ghi để xem chi tiết" /></Card>}
      </aside>
    </div>
  </>
}

function AttendanceMetric({ icon, label, value, tone }: { icon: ReactNode; label: string; value: number; tone: string }) {
  return <div className="attendance-metric"><span className={`tone-${tone}`}>{icon}</span><div><small>{label}</small><b>{value}</b><em>buổi học</em></div></div>
}

function RawStatusTag({ value }: { value: number }) {
  const colors = ['default', 'green', 'gold', 'orange', 'red'] as const
  return <Tag color={colors[value] ?? 'blue'}>Trạng thái {value}</Tag>
}

export function StudentFormRequestsPage() {
  const query = useQuery({ queryKey: ['student', 'form-requests'], queryFn: async () => {
    const [requests, templates] = await Promise.all([studentApi.formRequests(), studentApi.formTemplates()])
    return { requests, templates }
  } })
  return <div className="student-forms-page"><div className="student-page-heading"><Typography.Title level={2}>Biểu mẫu và yêu cầu</Typography.Title><Typography.Text type="secondary">Xem các mẫu biểu được cung cấp và theo dõi lịch sử yêu cầu của bạn.</Typography.Text></div><QueryState query={query}>{data => <FormsAndRequestsView {...data} />}</QueryState></div>
}

function FormsAndRequestsView({ requests, templates }: { requests: FormRequest[]; templates: StudentFormTemplate[] }) {
  const [templateSearch, setTemplateSearch] = useState('')
  const [status, setStatus] = useState<number>()
  const [selectedId, setSelectedId] = useState<string | null>(requests[0]?.formRequestId ?? null)
  const selected = requests.find(item => item.formRequestId === selectedId)
  const statuses = [...new Set(requests.map(item => item.rawStatus))]
  const visibleTemplates = templates.filter(item => item.name.toLocaleLowerCase('vi').includes(templateSearch.trim().toLocaleLowerCase('vi')))
  const visibleRequests = requests.filter(item => status == null || item.rawStatus === status)
  return <>
    <div className="forms-tabs"><span className="active">Mẫu biểu</span><span>Yêu cầu của tôi</span></div>
    <div className="forms-layout">
      <Card className="form-templates-card" title="Danh sách mẫu biểu"><Input allowClear prefix={<SearchOutlined />} value={templateSearch} onChange={event => setTemplateSearch(event.target.value)} placeholder="Tìm kiếm mẫu biểu..." /><div className="form-template-list">{visibleTemplates.length ? visibleTemplates.map((item, index) => <div className="form-template-item" key={item.formTemplateId}><span className={`tone-${['blue','green','purple','orange'][index % 4]}`}><FileTextOutlined /></span><div><b>{item.name}</b><small>Mẫu biểu được cung cấp bởi nhà trường.</small></div>{item.safeDocumentUrl ? <a href={item.safeDocumentUrl} target="_blank" rel="noopener noreferrer">Xem mẫu</a> : <Tag>Chưa có tệp</Tag>}</div>) : <Empty description="Chưa có mẫu biểu được cung cấp" />}</div><div className="forms-count">Hiển thị {visibleTemplates.length} trong tổng số {templates.length} mẫu biểu</div></Card>
      <Card className="my-requests-card" title="Yêu cầu của tôi"><div className="request-toolbar"><Select allowClear value={status} onChange={setStatus} options={statuses.map(value => ({ value, label: `Trạng thái ${value}` }))} placeholder="Tất cả trạng thái" /><Button disabled title="Chức năng ghi ngoài phase hiện tại">+ Tạo yêu cầu</Button></div><div className="request-table-head"><b>Mẫu biểu</b><b>Ngày tạo</b><b>Cập nhật</b><b>Trạng thái</b><b>Người duyệt</b><span /></div>{visibleRequests.length ? visibleRequests.map(item => <button className={`request-row ${selectedId === item.formRequestId ? 'selected' : ''}`} key={item.formRequestId} onClick={() => setSelectedId(item.formRequestId)} onKeyDown={event => { if (event.key === 'Enter') setSelectedId(item.formRequestId) }}><span><b>{item.formTemplateName ?? 'Chưa có tên mẫu'}</b><small>{item.formRequestId.slice(0, 12)}</small></span><span>{dateTime(item.creationDate)}</span><span>{dateTime(item.updateDate)}</span><Tag color="blue">Trạng thái {item.rawStatus}</Tag><span>{item.approvalName || '—'}</span><RightOutlined /></button>) : <Empty description="Chưa có yêu cầu nào" />}<Alert type="info" showIcon message="Tạo, gửi, hủy yêu cầu, tải tệp đính kèm và chuyển trạng thái là chức năng ghi ngoài phase hiện tại." /></Card>
      <aside>{selected ? <Card className="request-detail-card" title="Chi tiết yêu cầu"><Tag color="blue">Trạng thái {selected.rawStatus}</Tag><Typography.Text>Mã yêu cầu: {selected.formRequestId.slice(0, 18)}</Typography.Text><section><h4>Thông tin yêu cầu</h4><ProfileInfoCardRows rows={[['Mẫu biểu', selected.formTemplateName ?? 'Chưa cập nhật'], ['Ngày tạo', dateTime(selected.creationDate)], ['Cập nhật', dateTime(selected.updateDate)], ['Trạng thái raw', String(selected.rawStatus)], ['Người duyệt', selected.approvalName || 'Chưa cập nhật'], ['Ghi chú', selected.note || '—']]} /></section>{selected.safeDocumentUrl && <a className="request-document-link" href={selected.safeDocumentUrl} target="_blank" rel="noopener noreferrer"><FileTextOutlined /> Xem tài liệu mẫu <RightOutlined /></a>}<section className="request-history"><h4>Lịch sử xử lý</h4><div><i /><span><b>{dateTime(selected.updateDate)}</b><small>{selected.approvalName || 'Hệ thống'} · Cập nhật yêu cầu.</small></span></div><div><i /><span><b>{dateTime(selected.creationDate)}</b><small>Tạo yêu cầu.</small></span></div></section></Card> : <Card><Empty description="Chọn yêu cầu để xem chi tiết" /></Card>}</aside>
    </div>
  </>
}
