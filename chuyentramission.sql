-- 1. Bảng Danh Mục Trạng Thái (sys_WorkflowStatuses)
IF OBJECT_ID('dbo.sys_WorkflowTransitions', 'U') IS NOT NULL DROP TABLE dbo.sys_WorkflowTransitions;
IF OBJECT_ID('dbo.sys_WorkflowStatuses', 'U') IS NOT NULL DROP TABLE dbo.sys_WorkflowStatuses;

CREATE TABLE sys_WorkflowStatuses (
    ProcessCode VARCHAR(50) NOT NULL,    -- Mã quy trình (VD: KHACH_TRA, TRA_NOI_BO, QT_CHUNG)
    StatusCode INT NOT NULL,             -- Mã trạng thái (Map đúng giá trị Enum)
    StatusName NVARCHAR(100) NOT NULL,   -- Tên hiển thị trạng thái
    IsTerminal BIT NOT NULL DEFAULT 0,   -- 1: Trạng thái kết thúc (Hoàn tất)
    Description NVARCHAR(500) NULL,      -- Ghi chú/Diễn giải nghiệp vụ chi tiết của trạng thái
    SortOrder INT NOT NULL DEFAULT 0,    -- Thứ tự sắp xếp hiển thị
    CONSTRAINT PK_sys_WorkflowStatuses PRIMARY KEY (ProcessCode, StatusCode)
);

-- 2. Bảng Ma Trận Chuyển Trạng Thái (sys_WorkflowTransitions)
CREATE TABLE sys_WorkflowTransitions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProcessCode VARCHAR(50) NOT NULL,
    FromStatus INT NOT NULL,
    ToStatus INT NOT NULL,
    ActionName NVARCHAR(150) NULL,       -- Tên thao tác / Nhãn hiển thị trên Nút bấm (Button UI)
    Description NVARCHAR(500) NULL,      -- Ghi chú quy trình / Điều kiện nghiệp vụ khi rẽ nhánh
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_Transition_From FOREIGN KEY (ProcessCode, FromStatus) 
        REFERENCES sys_WorkflowStatuses(ProcessCode, StatusCode),
    CONSTRAINT FK_Transition_To FOREIGN KEY (ProcessCode, ToStatus) 
        REFERENCES sys_WorkflowStatuses(ProcessCode, StatusCode),
    CONSTRAINT UK_Transition UNIQUE (ProcessCode, FromStatus, ToStatus)
);

-- ============================================================================
-- 1. KHACH_TRA: Danh mục Trạng Thái
-- ============================================================================
INSERT INTO sys_WorkflowStatuses (ProcessCode, StatusCode, StatusName, IsTerminal, SortOrder, Description) VALUES
('KHACH_TRA', 0,   N'Mới',                     0, 1,  N'Header mới được tạo. Chưa bắt đầu quy trình xử lý.'),
('KHACH_TRA', 10,  N'Chờ tạo phiếu bất thường', 0, 2,  N'Header đã sẵn sàng để tạo PhieuXuLyBatThuong cho các dòng PhieuTraHangCT.'),
('KHACH_TRA', 20,  N'Đã tạo phiếu bất thường',  0, 3,  N'Có ít nhất một dòng PhieuTraHangCT đã được liên kết với PhieuXuLyBatThuong (Trạng thái Roll-up cấp Header).'),
('KHACH_TRA', 30,  N'Đang xử lý QTChung',       0, 4,  N'Có ít nhất một PhieuXuLyBatThuong đang thực hiện QTChung và toàn bộ Header chưa hoàn tất.'),
('KHACH_TRA', 100, N'Hoàn tất',                 1, 5,  N'Toàn bộ các dòng xử lý thuộc Header đã kết thúc.'),
('KHACH_TRA', 900, N'Lỗi',                      0, 99, N'Quy trình đang ở trạng thái lỗi nghiệp vụ và cần xử lý/retry.');

-- ============================================================================
-- 2. TRA_NOI_BO: Danh mục Trạng Thái
-- ============================================================================
INSERT INTO sys_WorkflowStatuses (ProcessCode, StatusCode, StatusName, IsTerminal, SortOrder, Description) VALUES
('TRA_NOI_BO', 0,   N'Mới',                     0, 1,  N'Header mới được tạo. Chưa bắt đầu quy trình xử lý.'),
('TRA_NOI_BO', 10,  N'Chờ tạo phiếu bất thường', 0, 2,  N'Header đã sẵn sàng để tạo PhieuXuLyBatThuong cho các dòng PhieuTraHangCT.'),
('TRA_NOI_BO', 20,  N'Đã tạo phiếu bất thường',  0, 3,  N'Có ít nhất một dòng PhieuTraHangCT đã được liên kết với PhieuXuLyBatThuong (Trạng thái Roll-up cấp Header).'),
('TRA_NOI_BO', 30,  N'Đang xử lý QTChung',       0, 4,  N'Có ít nhất một PhieuXuLyBatThuong đang thực hiện QTChung và toàn bộ Header chưa hoàn tất.'),
('TRA_NOI_BO', 75,  N'Chờ giao lại bộ phận',    0, 5,  N'Chỉ áp dụng TraNoiBo. QTChung đã hoàn tất phần xử lý hàng, còn hàng OK cần giao lại cho bộ phận nhận.'),
('TRA_NOI_BO', 80,  N'Đã giao lại bộ phận',     0, 6,  N'Chỉ áp dụng TraNoiBo. Hàng OK đã được giao lại cho bộ phận nhận.'),
('TRA_NOI_BO', 100, N'Hoàn tất',                 1, 7,  N'Toàn bộ các dòng xử lý thuộc Header đã kết thúc.'),
('TRA_NOI_BO', 900, N'Lỗi',                      0, 99, N'Quy trình đang ở trạng thái lỗi nghiệp vụ và cần xử lý/retry.');

-- ============================================================================
-- 1. KHACH_TRA: Ma trận Chuyển Trạng Thái & Ghi Chú Luồng
-- ============================================================================
INSERT INTO sys_WorkflowTransitions (ProcessCode, FromStatus, ToStatus, ActionName, Description) VALUES
-- From Moi (0)
('KHACH_TRA', 0, 10,  N'Chuyển chờ tạo phiếu', N'Xác nhận Header hợp lệ, chuyển sang sẵn sàng tạo phiếu bất thường.'),
('KHACH_TRA', 0, 900, N'Báo lỗi',              N'Phát hiện lỗi dữ liệu khi khởi tạo Header.'),

-- From ChoTaoPhieuBatThuong (10)
('KHACH_TRA', 10, 20,  N'Tạo phiếu bất thường',N'Liên kết thành công ít nhất 1 dòng chi tiết PhieuTraHangCT với PhieuXuLyBatThuong.'),
('KHACH_TRA', 10, 900, N'Báo lỗi',             N'Lỗi hệ thống hoặc lỗi nghiệp vụ trong quá trình tạo phiếu bất thường.'),

-- From DaTaoPhieuBatThuong (20)
('KHACH_TRA', 20, 30,  N'Bắt đầu QTChung',     N'Chuyển phiếu bất thường sang luồng QTChung để xử lý hàng lỗi.'),
('KHACH_TRA', 20, 900, N'Báo lỗi',             N'Gặp sự cố khi khởi tạo luồng QTChung.'),

-- From DangXuLyQTChung (30)
('KHACH_TRA', 30, 100, N'Chốt hoàn tất',        N'Tất cả các dòng phiếu xử lý bất thường thuộc Header đã hoàn tất QTChung.'),
('KHACH_TRA', 30, 900, N'Báo lỗi',             N'Phát sinh lỗi nghiệp vụ trong quá trình thực hiện QTChung.'),

-- From Loi (900) - Retry Step
('KHACH_TRA', 900, 10, N'Retry Tạo phiếu BT',  N'Xử lý lỗi và thử lại bước Tạo phiếu bất thường cho các dòng chi tiết.'),
('KHACH_TRA', 900, 30, N'Retry Luồng QTChung',  N'Xử lý lỗi và thử lại quá trình thực hiện QTChung.');


-- ============================================================================
-- 2. TRA_NOI_BO: Ma trận Chuyển Trạng Thái & Ghi Chú Luồng
-- ============================================================================
INSERT INTO sys_WorkflowTransitions (ProcessCode, FromStatus, ToStatus, ActionName, Description) VALUES
-- From Moi (0)
('TRA_NOI_BO', 0, 10,  N'Chuyển chờ tạo phiếu', N'Xác nhận Header trả nội bộ hợp lệ.'),
('TRA_NOI_BO', 0, 900, N'Báo lỗi',              N'Phát hiện lỗi dữ liệu khi khởi tạo Header.'),

-- From ChoTaoPhieuBatThuong (10)
('TRA_NOI_BO', 10, 20,  N'Tạo phiếu bất thường',N'Liên kết thành công các dòng chi tiết trả nội bộ với phiếu bất thường.'),
('TRA_NOI_BO', 10, 900, N'Báo lỗi',             N'Lỗi hệ thống trong quá trình khởi tạo phiếu bất thường.'),

-- From DaTaoPhieuBatThuong (20)
('TRA_NOI_BO', 20, 30,  N'Bắt đầu QTChung',     N'Chuyển phiếu sang luồng xử lý hàng QTChung.'),
('TRA_NOI_BO', 20, 900, N'Báo lỗi',             N'Lỗi khi đưa phiếu vào luồng QTChung.'),

-- From DangXuLyQTChung (30)
('TRA_NOI_BO', 30, 75,  N'Giao trả bộ phận',    N'QTChung hoàn tất xử lý hàng. Còn hàng OK cần bàn giao lại cho bộ phận nhận.'),
('TRA_NOI_BO', 30, 100, N'Chốt hoàn tất trực tiếp', N'QTChung hoàn tất và không có hàng OK cần bàn giao lại.'),
('TRA_NOI_BO', 30, 900, N'Báo lỗi',             N'Phát sinh lỗi trong quá trình xử lý QTChung.'),

-- From ChoGiaoLaiBoPhan (75)
('TRA_NOI_BO', 75, 80,  N'Xác nhận đã giao hàng', N'Bộ phận nhận đã ký xác nhận nhận lại toàn bộ hàng OK.'),
('TRA_NOI_BO', 75, 900, N'Báo lỗi',             N'Lỗi hoặc từ chối khi bàn giao lại cho bộ phận.'),

-- From DaGiaoLaiBoPhan (80)
('TRA_NOI_BO', 80, 100, N'Chốt hoàn tất',        N'Xác nhận bàn giao hoàn thành, khép lại toàn bộ quy trình Header Trả nội bộ.'),
('TRA_NOI_BO', 80, 900, N'Báo lỗi',             N'Lỗi phát sinh ở bước đóng hồ sơ bàn giao.'),

-- From Loi (900) - Retry Step
('TRA_NOI_BO', 900, 10, N'Retry Tạo phiếu BT',  N'Xử lý lỗi và thử lại bước Tạo phiếu bất thường.'),
('TRA_NOI_BO', 900, 30, N'Retry Luồng QTChung',  N'Xử lý lỗi và quay lại luồng QTChung.');


-- ============================================================================
-- QT_CHUNG: Danh mục Trạng Thái Xử Lý Bất Thường
-- ============================================================================
INSERT INTO sys_WorkflowStatuses (ProcessCode, StatusCode, StatusName, IsTerminal, SortOrder, Description) VALUES
('QT_CHUNG', 0,   N'Mới',                      0, 1,  N'Phiếu xử lý bất thường mới được tạo, chưa bắt đầu xử lý.'),
('QT_CHUNG', 10,  N'Đã tạo phiếu bất thường',  0, 2,  N'Phiếu xử lý bất thường đã được tạo và liên kết thành công.'),
('QT_CHUNG', 20,  N'Đã định hướng',            0, 3,  N'Đã xác định hướng xử lý: TuChoiGiaoBu, ChiGiaoBu, hoặc CanRework.'),

-- Nhánh 1: Từ chối giao bù
('QT_CHUNG', 25,  N'Từ chối giao bù',          0, 4,  N'Xác định không phải lỗi thật, không thực hiện giao bù hay rework.'),

-- Nhánh 2: Chỉ giao bù
('QT_CHUNG', 30,  N'Chờ giao bù',              0, 5,  N'Đã tạo yêu cầu giao bù, đang chờ bộ phận giao bù hoàn tất.'),
('QT_CHUNG', 35,  N'Đã giao bù',               0, 6,  N'Quá trình giao bù sản phẩm đã hoàn tất.'),

-- Nhánh 3: Rework
('QT_CHUNG', 40,  N'Đã xuất kho Rework',       0, 7,  N'Hàng đã được xuất khỏi kho để đưa đi sửa chữa / rework.'),
('QT_CHUNG', 50,  N'Đã giao sản xuất',         0, 8,  N'Đã ghi nhận giao hàng cho bộ phận sản xuất/rework (không thay đổi tồn kho).'),
('QT_CHUNG', 60,  N'Đã QC xác nhận cuối',      0, 9,  N'QC đã hoàn thành kiểm tra và xác nhận kết quả cuối (OK / NG).'),
('QT_CHUNG', 70,  N'Đã nhập lại kho',          0, 10, N'Hàng NG sau QC đã được nhập lại kho (chỉ áp dụng khi SoLuongNG > 0).'),

-- Kết thúc / Hủy
('QT_CHUNG', 100, N'Hoàn tất',                 1, 11, N'Phiếu xử lý bất thường đã hoàn tất toàn bộ quy trình.'),
('QT_CHUNG', 900, N'Hủy',                      1, 99, N'Phiếu xử lý bất thường bị hủy.');


-- ============================================================================
-- QT_CHUNG: Ma trận Chuyển Trạng Thái & Ghi Chú Luồng Nghiệp Vụ
-- ============================================================================
INSERT INTO sys_WorkflowTransitions (ProcessCode, FromStatus, ToStatus, ActionName, Description) VALUES

-------------------------------------------------------------------------------
-- BƯỚC CHUNG (ChungMap)
-------------------------------------------------------------------------------
-- From Moi (0)
('QT_CHUNG', 0, 10,  N'Liên kết phiếu',       N'Xác nhận liên kết phiếu bất thường.'),
('QT_CHUNG', 0, 900, N'Hủy phiếu',            N'Hủy phiếu ở bước khởi tạo.'),

-- From DaTaoPhieuBatThuong (10)
('QT_CHUNG', 10, 20, N'Xác định hướng xử lý', N'Phân tích lỗi và chốt hướng xử lý (Từ chối / Chỉ giao bù / Rework).'),
('QT_CHUNG', 10, 900,N'Hủy phiếu',            N'Hủy phiếu khi chưa định hướng.'),

-------------------------------------------------------------------------------
-- NHÁNH 1: TỪ CHỐI GIAO BÙ (TuChoiGiaoBuMap)
-------------------------------------------------------------------------------
-- From DaDinhHuong (20) -> TuChoiGiaoBu (25)
('QT_CHUNG', 20, 25, N'Chuyển từ chối giao bù', N'Xác nhận không phải lỗi nhà máy/sản xuất, từ chối giao bù.'),
('QT_CHUNG', 20, 900,N'Hủy phiếu',              N'Hủy phiếu ở bước phân loại.'),

-- From TuChoiGiaoBu (25) -> HoanTat (100)
('QT_CHUNG', 25, 100,N'Đóng phiếu',             N'Hoàn tất quy trình do từ chối giao bù.'),

-------------------------------------------------------------------------------
-- NHÁNH 2: CHỈ GIAO BÙ (ChiGiaoBuMap)
-------------------------------------------------------------------------------
-- From DaDinhHuong (20) -> ChoGiaoBu (30)
('QT_CHUNG', 20, 30, N'Yêu cầu giao bù',      N'Chuyển sang nhánh giao bù sản phẩm.'),

-- From ChoGiaoBu (30)
('QT_CHUNG', 30, 35, N'Xác nhận đã giao bù',  N'Xác nhận kho/xuất hàng đã giao đủ hàng bù.'),
('QT_CHUNG', 30, 900,N'Hủy phiếu',            N'Hủy yêu cầu giao bù.'),

-- From DaGiaoBu (35) -> HoanTat (100)
('QT_CHUNG', 35, 100,N'Đóng phiếu',             N'Hoàn tất quy trình sau khi giao bù thành công.'),

-------------------------------------------------------------------------------
-- NHÁNH 3: CAN REWORK (ReworkMap)
-------------------------------------------------------------------------------
-- From DaDinhHuong (20) -> DaXuatKhoRework (40)
('QT_CHUNG', 20, 40, N'Xuất kho Rework',       N'Duyệt xuất kho hàng lỗi đưa đi sửa chữa.'),

-- From DaXuatKhoRework (40)
('QT_CHUNG', 40, 40, N'Xuất kho bổ sung',       N'Cho phép xuất kho thêm/bổ sung hàng đi rework.'),
('QT_CHUNG', 40, 50, N'Bàn giao sản xuất',     N'Ghi nhận đã bàn giao hàng cho bộ phận sản xuất/xưởng rework.'),
('QT_CHUNG', 40, 900,N'Hủy phiếu',            N'Hủy phiếu trong quá trình xuất Rework.'),

-- From DaGiaoSanXuat (50)
('QT_CHUNG', 50, 60, N'QC Kiểm tra cuối',      N'QC kiểm tra đánh giá lại hàng sau Rework (OK/NG).'),
('QT_CHUNG', 50, 900,N'Hủy phiếu',            N'Hủy phiếu ở giai đoạn sản xuất.'),

-- From DaQCXacNhanCuoi (60)
('QT_CHUNG', 60, 100,N'Chốt hoàn tất (100% OK)',N'Hoàn tất quy trình trực tiếp nếu toàn bộ hàng Rework đạt OK (NG = 0).'),
('QT_CHUNG', 60, 70, N'Nhập kho hàng NG',      N'Chuyển sang nhập kho hàng hỏng/phế phẩm nếu có số lượng NG > 0.'),
('QT_CHUNG', 60, 900,N'Hủy phiếu',             N'Hủy phiếu sau khi có kết quả QC.'),

-- From DaNhapLaiKho (70)
('QT_CHUNG', 70, 70, N'Nhập kho bổ sung',       N'Cập nhật/nhập kho tiếp các lô hàng NG còn lại.'),
('QT_CHUNG', 70, 100,N'Chốt hoàn tất',         N'Hoàn tất quy trình sau khi đã hoàn thành nhập kho hàng NG.'),
('QT_CHUNG', 70, 900,N'Hủy phiếu',             N'Hủy phiếu ở bước nhập kho NG.');