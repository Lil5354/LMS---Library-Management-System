USE MASTER
GO
DROP DATABASE IF EXISTS LIBRARYM
GO
CREATE DATABASE LIBRARYM
GO
USE LIBRARYM
GO
--TÁC GỈA 
CREATE TABLE AUTHORS (
    AUTHORID INT PRIMARY KEY IDENTITY(1, 1),
    FULLNAME NVARCHAR(100) NOT NULL,
    BIOGRAPHY NVARCHAR(MAX)
);
GO
--NHÀ XUẤT BẢN
CREATE TABLE PUBLISHERS (
    PUBLISHERID INT PRIMARY KEY IDENTITY(1, 1),
    [NAME] NVARCHAR(100) NOT NULL,
    [ADDRESS] NVARCHAR(255),
    PHONE NVARCHAR(15),
    EMAIL NVARCHAR(100)
);
GO
CREATE TABLE CATEGORIES (
    CATEGORYID INT PRIMARY KEY IDENTITY(1, 1),
    [NAME] NVARCHAR(100) NOT NULL,
    [DESCRIPTION] NVARCHAR(MAX)
);
GO
CREATE TABLE BOOKS (
    BOOKID INT PRIMARY KEY IDENTITY(1, 1),
    TITLE NVARCHAR(255) NOT NULL,
    AUTHORID INT NOT NULL,
    PUBLISHERID INT NOT NULL,
    CATEGORYID INT NOT NULL,
    PUBLICATIONYEAR INT,
	DATEADDB DATE NOT NULL DEFAULT GETDATE(),
    BORROWEDCOUNT INT DEFAULT 0,
	DESCRIPTION NVARCHAR(MAX),
	STATUS INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_BOOKS_AUTHORS FOREIGN KEY (AUTHORID) REFERENCES AUTHORS(AUTHORID),
    CONSTRAINT FK_BOOKS_PUBLISHERS FOREIGN KEY (PUBLISHERID) REFERENCES PUBLISHERS(PUBLISHERID),
    CONSTRAINT FK_BOOKS_CATEGORIES FOREIGN KEY (CATEGORYID) REFERENCES CATEGORIES(CATEGORYID)
);
GO
CREATE TABLE READERS (
    READERID INT PRIMARY KEY IDENTITY(1, 1),
    FULLNAME NVARCHAR(100) NOT NULL,
    EMAIL NVARCHAR(100),
    PHONE NVARCHAR(15),
    [ADDRESS] NVARCHAR(255),
    DATEOFBIRTH DATE,
    REGISTRATIONDATE DATE DEFAULT GETDATE(),
	[PASSWORD] NVARCHAR(50) NOT NULL CHECK(LEN([PASSWORD]) >= 6) DEFAULT 'lms2025'

);
GO
CREATE TABLE LIBRARIANS (
    LIBRARIANID INT PRIMARY KEY IDENTITY(1, 1),
	IDSTAFF    NVARCHAR(10) UNIQUE NOT NULL,
	FULLNAME   NVARCHAR(50) NOT NULL,
	EMAIL      NVARCHAR(50) UNIQUE NOT NULL,
	PHONE      NVARCHAR(11) UNIQUE NOT NULL,
	DOB        DATE NOT NULL CHECK (DOB <= GETDATE()),
	[PASSWORD] NVARCHAR(50) NOT NULL CHECK(LEN([PASSWORD]) >= 6) DEFAULT 'lms2025',
	[ROLE]     NVARCHAR(20) CHECK([ROLE] IN ('Manager', 'Librarian')),
	STATUS	   INT NOT NULL DEFAULT 1
);
GO
-- Trigger for auto-generating ID LIBRARIANS
CREATE TRIGGER TRG_INSERTIDSTAFF
ON LIBRARIANS
INSTEAD OF INSERT
AS
BEGIN
	DECLARE @NEWID INT;
	DECLARE @NEWIDSTAFF NVARCHAR(10);

	SELECT @NEWID = ISNULL(MAX(CAST(SUBSTRING(IDSTAFF, 3, LEN(IDSTAFF)) AS INT)), 0) + 1
	FROM LIBRARIANS;

	SET @NEWIDSTAFF = 'LB' + RIGHT('000' + CAST(@NEWID AS NVARCHAR(3)), 3);

	INSERT INTO LIBRARIANS (IDSTAFF, FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS)
	SELECT @NEWIDSTAFF, FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS
	FROM INSERTED;
END;

GO
CREATE TABLE BORROWINGTICKETS (
    TICKETID INT PRIMARY KEY IDENTITY(1, 1),
    READERID INT NOT NULL,
    LIBRARIANID NVARCHAR(10) NULL,
    BOOKID INT NOT NULL, -- Added BOOKID column
    BORROWDATE DATE DEFAULT GETDATE(),
    DUEDATE DATE NOT NULL,
    RETURNDATE DATE,
    [STATUS] NVARCHAR(50) NULL CHECK([STATUS] IN ('Borrowing', 'Restored','Waiting')) DEFAULT 'Waiting',
    APPROVAL_STATUS NVARCHAR(50) NOT NULL CHECK(APPROVAL_STATUS IN ('Waiting', 'Approve', 'Decline')) DEFAULT 'Waiting',
    CONSTRAINT FK_BORROWINGTICKETS_READERS FOREIGN KEY (READERID) REFERENCES READERS(READERID),
    CONSTRAINT FK_BORROWINGTICKETS_LIBRARIANS FOREIGN KEY (LIBRARIANID) REFERENCES LIBRARIANS(IDSTAFF),
    CONSTRAINT FK_BORROWINGTICKETS_BOOKS FOREIGN KEY (BOOKID) REFERENCES BOOKS(BOOKID)
);
GO
CREATE TABLE RETURNTICKETS (
    RETURNID INT PRIMARY KEY IDENTITY(1, 1),
    TICKETID INT NOT NULL,
    LIBRARIANID NVARCHAR(10) NOT NULL,
    RETURNDATE DATE DEFAULT GETDATE(),
    FINEAMOUNT DECIMAL(10, 2) DEFAULT 0 CHECK (FINEAMOUNT >= 0),
    CONSTRAINT FK_RETURNTICKETS_BORROWINGTICKETS FOREIGN KEY (TICKETID) REFERENCES BORROWINGTICKETS(TICKETID),
    CONSTRAINT FK_RETURNTICKETS_LIBRARIANS FOREIGN KEY (LIBRARIANID) REFERENCES LIBRARIANS(IDSTAFF)
);
GO
--TRIGGER
CREATE TRIGGER TRG_BORROWINGTICKETS_AND_RETURNTICKETS
ON BORROWINGTICKETS
AFTER INSERT, UPDATE
AS
BEGIN
    -- Kiểm tra nếu có bản ghi được cập nhật hoặc chèn
    IF EXISTS (SELECT 1 FROM INSERTED)
    BEGIN
        -- Xử lý khi APPROVAL_STATUS được cập nhật thành 'Approve'
        IF UPDATE(APPROVAL_STATUS)
        BEGIN
            -- Cập nhật STATUS thành 'Borrowing' nếu APPROVAL_STATUS là 'Approve'
            UPDATE BORROWINGTICKETS
            SET [STATUS] = 'Borrowing'
            WHERE TICKETID IN (
                SELECT TICKETID
                FROM INSERTED
                WHERE APPROVAL_STATUS = 'Approve'
            );

            -- Tăng BORROWEDCOUNT của bảng BOOKS lên 1
            UPDATE BOOKS
            SET BORROWEDCOUNT = BORROWEDCOUNT + 1
            WHERE BOOKID IN (
                SELECT BOOKID
                FROM INSERTED
                WHERE APPROVAL_STATUS = 'Approve'
            );
        END;

        -- Xử lý khi STATUS là 'Borrowing' hoặc 'Restored'
        IF EXISTS (
            SELECT 1
            FROM INSERTED
            WHERE [STATUS] IN ('Borrowing', 'Restored') AND APPROVAL_STATUS != 'Approve'
        )
        BEGIN
            -- Cập nhật APPROVAL_STATUS thành 'Approve' nếu STATUS là 'Borrowing' hoặc 'Restored'
            UPDATE BORROWINGTICKETS
            SET APPROVAL_STATUS = 'Approve'
            WHERE TICKETID IN (
                SELECT TICKETID
                FROM INSERTED
                WHERE [STATUS] IN ('Borrowing', 'Restored') AND APPROVAL_STATUS != 'Approve'
            );
        END;

        -- Xử lý khi APPROVAL_STATUS là 'Decline'
        IF EXISTS (
            SELECT 1
            FROM INSERTED
            WHERE APPROVAL_STATUS = 'Decline' AND [STATUS] IS NOT NULL
        )
        BEGIN
            -- Cập nhật STATUS thành NULL nếu APPROVAL_STATUS là 'Decline'
            UPDATE BORROWINGTICKETS
            SET [STATUS] = NULL
            WHERE TICKETID IN (
                SELECT TICKETID
                FROM INSERTED
                WHERE APPROVAL_STATUS = 'Decline' or APPROVAL_STATUS = 'Waiting'
            );
        END;
    END;
END;
GO
CREATE TRIGGER TRG_RETURNTICKETS_UPDATE_STATUS
ON RETURNTICKETS
AFTER INSERT
AS
BEGIN
    -- Cập nhật STATUS của BORROWINGTICKETS thành 'Restored'
    UPDATE BORROWINGTICKETS
    SET [STATUS] = 'Restored'
    WHERE TICKETID IN (SELECT TICKETID FROM INSERTED);
END;
GO
----------------------------------------------------------------------------------------------------------
--INSERT
GO
INSERT INTO AUTHORS (FULLNAME, BIOGRAPHY)
VALUES 
    (N'Harper Lee', N'Nhà văn người Mỹ, tác giả của "To Kill a Mockingbird".'),
    (N'George Orwell', N'Nhà văn người Anh, tác giả của "1984" và "Animal Farm".'),
    (N'J.K. Rowling', N'Nhà văn người Anh, tác giả của loạt truyện "Harry Potter".'),
    (N'Nguyễn Nhật Ánh', N'Nhà văn người Việt Nam, tác giả của "Tôi thấy hoa vàng trên cỏ xanh".'),
    (N'Paulo Coelho', N'Nhà văn người Brazil, tác giả của "Nhà giả kim".'),
	(N'Mark Twain', N'Nhà văn người Mỹ, tác giả của "Những cuộc phiêu lưu của Tom Sawyer".'),
    (N'Jane Austen', N'Nhà văn người Anh, tác giả của "Pride and Prejudice".'),
    (N'F. Scott Fitzgerald', N'Nhà văn người Mỹ, tác giả của "The Great Gatsby".'),
	(N'Leo Tolstoy', N'Nhà văn người Nga, tác giả của "Chiến tranh và Hòa bình".'),
    (N'Ernest Hemingway', N'Nhà văn người Mỹ, tác giả của "The Old Man and the Sea".'),
    (N'Victor Hugo', N'Nhà văn người Pháp, tác giả của "Những người khốn khổ".'),
    (N'J.R.R. Tolkien', N'Nhà văn người Anh, tác giả của "Chúa tể những chiếc nhẫn".'),
    (N'Agatha Christie', N'Nhà văn người Anh, tác giả của nhiều tiểu thuyết trinh thám nổi tiếng.'),
	(N'Nguyễn Du', N'Nhà thơ lớn của Việt Nam, tác giả của "Truyện Kiều".'),
    (N'Hồ Chí Minh', N'Chủ tịch Hồ Chí Minh, nhà cách mạng và nhà văn hóa lớn của Việt Nam.'),
    (N'Nam Cao', N'Nhà văn hiện thực phê phán, tác giả của "Chí Phèo".'),
    (N'Xuân Diệu', N'Nhà thơ nổi tiếng của phong trào Thơ Mới.'),
    (N'Vũ Trọng Phụng', N'Nhà văn hiện thực phê phán, tác giả của "Số đỏ".'),
	(N'Fujiko F. Fujio', N'Tác giả truyện tranh Doraemon.'),
    (N'Gosho Aoyama', N'Tác giả truyện tranh Thám tử lừng danh Conan.'),
    (N'Eiichiro Oda', N'Tác giả truyện tranh One Piece.'),
    (N'Masashi Kishimoto', N'Tác giả truyện tranh Naruto.'),
    (N'Akira Toriyama', N'Tác giả truyện tranh Dragon Ball.');
GO
INSERT INTO PUBLISHERS ([NAME], [ADDRESS], PHONE, EMAIL)
VALUES 
    (N'Nhà xuất bản Văn học', N'Hà Nội, Việt Nam', '0123456789', 'nxbvanhoc@example.com'),
    (N'Nhà xuất bản Kim Đồng', N'Hồ Chí Minh, Việt Nam', '0987654321', 'nxbkimdong@example.com'),
    (N'Nhà xuất bản Trẻ', N'Hồ Chí Minh, Việt Nam', '0369852147', 'nxbtre@example.com'),
    (N'Nhà xuất bản Hội Nhà Văn', N'Hà Nội, Việt Nam', '0321654987', 'nxbhoinhavan@example.com'),
    (N'Nhà xuất bản Thế Giới', N'Hà Nội, Việt Nam', '0963258741', 'nxbthegioi@example.com');
GO
INSERT INTO CATEGORIES ([NAME], [DESCRIPTION])
VALUES 
    (N'Văn học', N'Sách về văn học trong nước và quốc tế.'),
    (N'Khoa học', N'Sách về khoa học tự nhiên và xã hội.'),
    (N'Tiểu thuyết', N'Sách tiểu thuyết nổi tiếng.'),
    (N'Kỹ năng sống', N'Sách về kỹ năng sống và phát triển bản thân.'),
    (N'Thiếu nhi', N'Sách dành cho thiếu nhi.'),
	 (N'Lịch sử', N'Sách về lịch sử thế giới và Việt Nam.'),
    (N'Kinh tế', N'Sách về kinh tế học, tài chính và quản lý.'),
    (N'Tâm lý học', N'Sách về tâm lý học và phát triển cá nhân.'),
    (N'Công nghệ thông tin', N'Sách về lập trình, công nghệ thông tin và khoa học máy tính.'),
    (N'Nghệ thuật', N'Sách về nghệ thuật, hội họa và âm nhạc.'),
	(N'Truyện tranh', N'Sách truyện tranh dành cho mọi lứa tuổi.'),
    (N'Tình cảm', N'Sách về tình yêu, tình bạn, và các mối quan hệ.');
GO
INSERT INTO BOOKS (TITLE, AUTHORID, PUBLISHERID, CATEGORYID, PUBLICATIONYEAR, BORROWEDCOUNT, DESCRIPTION, DATEADDB)
VALUES 
    (N'To Kill a Mockingbird', 1, 1, 1, 1960, 10, N'Cuốn tiểu thuyết kinh điển về phân biệt chủng tộc và công lý tại miền Nam nước Mỹ.', '2023-10-01'),
    (N'1984', 2, 2, 1, 1949, 5, N'Tác phẩm dystopia nổi tiếng về một xã hội bị kiểm soát bởi chính phủ toàn trị.', '2023-10-02'),
    (N'Harry Potter và Hòn đá Phù thủy', 3, 3, 3, 1997, 15, N'Cuốn đầu tiên trong loạt truyện giả tưởng nổi tiếng về cậu bé phù thủy Harry Potter.', '2023-10-03'),
    (N'Tôi thấy hoa vàng trên cỏ xanh', 4, 4, 3, 2010, 20, N'Cuốn sách về tuổi thơ, tình bạn và tình yêu trong sáng.', '2023-10-04'),
    (N'Nhà giả kim', 5, 5, 4, 1988, 12, N'Hành trình tìm kiếm ý nghĩa cuộc sống và ước mơ của cậu bé chăn cừu Santiago.', '2023-10-05'),
    (N'Animal Farm', 2, 2, 1, 1945, 8, N'Ngụ ngôn chính trị về sự trỗi dậy của chủ nghĩa độc tài.', '2024-10-06'),
    (N'Harry Potter và Phòng chứa Bí mật', 3, 3, 3, 1998, 12, N'Cuốn thứ hai trong loạt truyện Harry Potter, kể về những bí mật tại trường Hogwarts.', '2023-10-07'),
    (N'Harry Potter và Tên tù nhân ngục Azkaban', 3, 3, 3, 1999, 10, N'Cuốn thứ ba trong loạt truyện Harry Potter, với sự xuất hiện của tên tù nhân Sirius Black.', '2023-10-08'),
    (N'Cho tôi xin một vé đi tuổi thơ', 4, 4, 3, 2008, 15, N'Cuốn sách về những ký ức tuổi thơ đẹp đẽ và hồn nhiên.', '2023-10-09'),
    (N'Mắt biếc', 4, 4, 12, 1990, 18, N'Chuyện tình cảm động và sâu sắc về tình yêu tuổi học trò.', '2024-10-10'),
    (N'Cô gái đến từ hôm qua', 4, 4, 3, 1989, 20, N'Cuốn sách về tình yêu và sự trưởng thành.', '2023-10-11'),
    (N'Brida', 5, 5, 4, 1990, 7, N'Hành trình khám phá bản thân và tâm linh của cô gái trẻ Brida.', '2023-10-12'),
    (N'Veronika quyết chết', 5, 5, 4, 1998, 9, N'Cuốn sách về ý nghĩa cuộc sống và sự lựa chọn.', '2023-10-13'),
    (N'Ông già và biển cả', 1, 1, 1, 1952, 6, N'Cuốn tiểu thuyết về cuộc chiến đấu giữa con người và thiên nhiên.', '2024-10-14'),
    (N'Đắc nhân tâm', 5, 5, 4, 1936, 25, N'Cuốn sách kinh điển về nghệ thuật thu phục lòng người.', '2025-1-15'),
	(N'Truyện Kiều', 14, 1, 1, 1820, 30, N'Tác phẩm kinh điển của Nguyễn Du, kể về cuộc đời của Thúy Kiều.', '2023-10-01'),
    (N'Nhật ký trong tù', 15, 2, 1, 1942, 20, N'Tập thơ của Chủ tịch Hồ Chí Minh viết trong thời gian bị giam cầm.', '2023-10-02'),
    (N'Chí Phèo', 16, 3, 1, 1941, 25, N'Tác phẩm kinh điển của Nam Cao về số phận bi thảm của người nông dân.', '2023-10-03'),
    (N'Thơ Xuân Diệu', 17, 4, 1, 1938, 15, N'Tuyển tập thơ của Xuân Diệu, nhà thơ tiêu biểu của phong trào Thơ Mới.', '2023-10-04'),
    (N'Số đỏ', 18, 5, 1, 1936, 18, N'Tác phẩm châm biếm xã hội của Vũ Trọng Phụng.', '2023-10-05'),
    (N'Lịch sử Việt Nam', 14, 1, 6, 2020, 10, N'Cuốn sách tổng hợp lịch sử Việt Nam từ thời kỳ dựng nước đến nay.', '2023-10-06'),
    (N'Kinh tế học vĩ mô', 15, 2, 7, 2015, 12, N'Giáo trình kinh tế học vĩ mô dành cho sinh viên đại học.', '2023-10-07'),
    (N'Tâm lý học đám đông', 16, 3, 8, 2018, 8, N'Cuốn sách phân tích tâm lý học đám đông và các hiện tượng xã hội.', '2023-10-08'),
    (N'Lập trình Python', 17, 4, 9, 2021, 20, N'Sách hướng dẫn lập trình Python từ cơ bản đến nâng cao.', '2023-10-09'),
    (N'Nghệ thuật hội họa', 18, 5, 10, 2019, 15, N'Cuốn sách giới thiệu về các trường phái hội họa nổi tiếng thế giới.', '2023-10-10'),
	(N'Doraemon', 19, 3, 11, 1969, 50, N'Bộ truyện tranh nổi tiếng về chú mèo máy Doraemon.', '2023-10-15'),
    (N'Thám tử lừng danh Conan', 20, 4, 11, 1994, 40, N'Bộ truyện tranh trinh thám về thám tử Conan.', '2023-10-16'),
    (N'One Piece', 21, 5, 11, 1997, 60, N'Bộ truyện tranh phiêu lưu về hành trình tìm kho báu của Luffy.', '2023-10-17'),
    (N'Naruto', 22, 1, 11, 1999, 55, N'Bộ truyện tranh về ninja Naruto và ước mơ trở thành Hokage.', '2023-10-18'),
    (N'Dragon Ball', 23, 2, 11, 1984, 45, N'Bộ truyện tranh về cuộc phiêu lưu của Goku và các viên ngọc rồng.', '2023-10-19'),
	(N'Ngày xưa có một chuyện tình', 4, 3, 12, 2016, 35, N'Cuốn sách về tình yêu và sự hy sinh.', '2023-10-25');
    
GO
INSERT INTO BOOKS (TITLE, AUTHORID, PUBLISHERID, CATEGORYID, PUBLICATIONYEAR, BORROWEDCOUNT, DESCRIPTION, DATEADDB, STATUS)
VALUES 
	(N'Chiến tranh và Hòa bình', 9, 1, 1, 1869, 5, N'Cuốn tiểu thuyết sử thi về xã hội Nga thời Napoleon.', '2023-10-21', 1),
    (N'The Old Man and the Sea', 10, 2, 1, 1952, 8, N'Cuốn tiểu thuyết về cuộc chiến đấu giữa con người và thiên nhiên.', '2023-10-22', 0),
    (N'Những người khốn khổ', 11, 3, 1, 1862, 12, N'Cuốn tiểu thuyết về xã hội Pháp thế kỷ 19.', '2023-10-23', 1),
    (N'Chúa tể những chiếc nhẫn', 12, 4, 3, 1954, 20, N'Bộ tiểu thuyết giả tưởng kinh điển về thế giới Trung Địa.', '2023-10-24', 0),
    (N'Án mạng trên chuyến tàu tốc hành Phương Đông', 13, 5, 3, 1934, 15, N'Cuốn tiểu thuyết trinh thám nổi tiếng của Agatha Christie.', '2023-10-25', 1),
    (N'Harry Potter và Bảo bối Tử thần', 3, 3, 3, 2007, 18, N'Cuốn cuối cùng trong loạt truyện Harry Potter.', '2023-10-26', 0),
    (N'Tiếng gọi nơi hoang dã', 10, 2, 1, 1903, 10, N'Cuốn tiểu thuyết về chú chó Buck và cuộc sống hoang dã.', '2023-10-27', 1),
    (N'Bố già', 5, 5, 3, 1969, 25, N'Cuốn tiểu thuyết về gia đình mafia Corleone.', '2023-10-28', 0),
    (N'Đồi gió hú', 7, 4, 1, 1847, 7, N'Cuốn tiểu thuyết tình cảm đầy kịch tính.', '2023-10-29', 1),
    (N'Trại súc vật', 2, 2, 1, 1945, 9, N'Ngụ ngôn chính trị về sự trỗi dậy của chủ nghĩa độc tài.', '2023-10-30', 0);
GO
INSERT INTO READERS (FULLNAME, EMAIL, PHONE, [ADDRESS], DATEOFBIRTH, REGISTRATIONDATE,  [PASSWORD])
VALUES 
   (N'John Smith', 'johnsmith@example.com', '0912345678', N'Hanoi', '2000-01-01', '2023-10-01', 'password123'),
    (N'Emily Johnson', 'emilyjohnson@example.com', '0987654321', N'Ho Chi Minh City', '1999-05-15', '2023-10-02', 'emily2023'),
    (N'Michael Brown', 'michaelbrown@example.com', '0369852147', N'Da Nang', '2001-08-20', '2023-10-03', 'brown2001'),
    (N'Sarah Davis', 'sarahdavis@example.com', '0321654987', N'Hai Phong', '2002-03-10', '2023-10-04', 'davis2002'),
    (N'David Wilson', 'davidwilson@example.com', '0963258741', N'Can Tho', '2003-12-25', '2023-10-05', 'wilson2003'),
    (N'Jennifer Lee', 'jenniferlee@example.com', '0912345679', N'Hanoi', '1998-06-12', '2023-11-01', 'jennifer98'),
    (N'Robert Taylor', 'roberttaylor@example.com', '0987654322', N'Ho Chi Minh City', '1997-09-22', '2023-11-02', 'robert1997'),
    (N'Jessica Martin', 'jessicamartin@example.com', '0369852148', N'Da Nang', '2000-04-15', '2023-11-03', 'jessica2000'),
    (N'Thomas Anderson', 'thomasanderson@example.com', '0321654988', N'Hai Phong', '2001-10-30', '2023-11-04', 'anderson01'),
    (N'Linda White', 'lindawhite@example.com', '0963258742', N'Can Tho', '2002-07-05', '2023-11-05', 'linda2002');
GO
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
    (N'Võ Đăng Khoa',			'khoavd2809@gmail.com',	'0843019548', '2004-09-28', 'khoavo123',		'Manager', 1)
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
	(N'Dương Thị Thanh Thảo',	'thaott26@gmail.com',	'0902234567', '2003-06-26', 'pupu123',			'Manager', 1)
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
	(N'Hoàng Văn Thiên',		'hvt2003@gmail.com',	'0903234567', '2003-05-15', 'chillguy1',		'Librarian', 1)
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
    (N'Lê Thiện Nhân',			'nhanle@gmail.com',		'0904234567', '2001-12-12', 'cuchuoi2xu',		'Librarian', 1)
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
    (N'Từ Tuấn Sang',			'tsang@gmail.com',		'0905234567', '2005-03-15', 'tsang123',			'Librarian', 1)
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
    (N'Nguyễn Thành Đạt',		'dathphong@gmail.com',	'0906234567', '2007-11-20', 'hoangtusitinh',	'Librarian', 1)
INSERT INTO LIBRARIANS (FULLNAME, EMAIL, PHONE, DOB, [PASSWORD], [ROLE], STATUS) VALUES 
    (N'Nguyễn Giang Gia Huy',	'huybo@gmail.com',		'0907234567', '2006-08-05', 'huybo123',			'Librarian', 1)
GO
INSERT INTO BORROWINGTICKETS (READERID, LIBRARIANID, BOOKID, BORROWDATE, DUEDATE, RETURNDATE, STATUS, APPROVAL_STATUS)
VALUES 
(1, 'LB001', 5, '2022-01-10', '2022-01-24', '2022-01-23', 'Restored', 'Approve'),
(2, 'LB002', 12, '2022-02-15', '2022-03-01', '2022-03-02', 'Restored', 'Approve'),
(3, 'LB003', 18, '2022-03-20', '2022-04-03', NULL, 'Borrowing', 'Approve'),
(4, 'LB004', 23, '2022-04-05', '2022-04-19', '2022-04-20', 'Restored', 'Approve'),
(5, 'LB005', 31, '2022-05-12', '2022-05-26', '2022-05-25', 'Restored', 'Approve'),
(1, 'LB001', 7, '2022-06-18', '2022-07-02', '2022-07-03', 'Restored', 'Approve'),
(2, 'LB002', 14, '2022-07-22', '2022-08-05', NULL, 'Borrowing', 'Approve'),
(3, 'LB003', 27, '2022-08-30', '2022-09-13', '2022-09-12', 'Restored', 'Approve'),
(4, 'LB004', 35, '2022-09-15', '2022-09-29', '2022-09-30', 'Restored', 'Approve'),
(5, 'LB005', 40, '2022-10-10', '2022-10-24', NULL, 'Borrowing', 'Approve'),
(2, 'LB002', 3, '2023-01-05', '2023-01-19', '2023-01-18', 'Restored', 'Approve'),
(3, 'LB003', 16, '2023-02-14', '2023-02-28', '2023-03-01', 'Restored', 'Approve'),
(4, 'LB004', 22, '2023-03-20', '2023-04-03', NULL, 'Borrowing', 'Approve'),
(5, 'LB005', 29, '2023-04-25', '2023-05-09', '2023-05-10', 'Restored', 'Approve'),
(1, 'LB001', 8, '2023-05-30', '2023-06-13', '2023-06-12', 'Restored', 'Approve'),
(2, 'LB002', 11, '2023-06-15', '2023-06-29', '2023-06-30', 'Restored', 'Approve'),
(3, 'LB003', 19, '2023-07-22', '2023-08-05', NULL, 'Borrowing', 'Approve'),
(4, 'LB004', 26, '2023-08-10', '2023-08-24', '2023-08-23', 'Restored', 'Approve'),
(5, 'LB005', 33, '2023-09-05', '2023-09-19', '2023-09-20', 'Restored', 'Approve'),
(1, 'LB001', 38, '2023-10-12', '2023-10-26', NULL, 'Borrowing', 'Approve'),
(3, 'LB003', 2, '2024-01-08', '2024-01-22', '2024-01-21', 'Restored', 'Approve'),
(4, 'LB004', 15, '2024-02-14', '2024-02-28', '2024-02-29', 'Restored', 'Approve'),
(5, 'LB005', 21, '2024-03-10', '2024-03-24', NULL, 'Borrowing', 'Approve'),
(1, 'LB001', 24, '2024-04-15', '2024-04-29', '2024-04-30', 'Restored', 'Approve'),
(2, 'LB002', 30, '2024-05-20', '2024-06-03', '2024-06-02', 'Restored', 'Approve'),
(3, 'LB003', 6, '2024-06-25', '2024-07-09', '2024-07-10', 'Restored', 'Approve'),
(4, 'LB004', 13, '2024-07-30', '2024-08-13', NULL, 'Borrowing', 'Approve'),
(5, 'LB005', 28, '2024-08-15', '2024-08-29', '2024-08-28', 'Restored', 'Approve'),
(1, 'LB001', 34, '2024-09-10', '2024-09-24', '2024-09-25', 'Restored', 'Approve'),
(2, 'LB002', 39, '2024-10-05', '2024-10-19', NULL, 'Borrowing', 'Approve'),
(4, 'LB004', 1, '2025-01-03', '2025-01-17', '2025-01-16', 'Restored', 'Approve'),
(5, 'LB005', 9, '2025-02-07', '2025-02-21', '2025-02-22', 'Restored', 'Approve'),
(1, 'LB001', 17, '2025-03-12', '2025-03-26', NULL, 'Borrowing', 'Approve'),
(2, 'LB002', 20, '2025-04-18', '2025-05-02', '2025-05-03', 'Restored', 'Approve'),
(3, 'LB003', 25, '2025-05-23', '2025-06-06', '2025-06-05', 'Restored', 'Approve'),
(4, 'LB004', 32, '2025-06-28', '2025-07-12', '2025-07-13', 'Restored', 'Approve'),
(5, 'LB005', 36, '2025-07-30', '2025-08-13', NULL, 'Borrowing', 'Approve'),
(1, 'LB001', 4, '2025-08-15', '2025-08-29', '2025-08-28', 'Restored', 'Approve'),
(2, 'LB002', 10, '2025-09-20', '2025-10-04', '2025-10-05', 'Restored', 'Approve'),
(3, 'LB003', 37, '2025-10-25', '2025-11-08', NULL, 'Borrowing', 'Approve');
INSERT INTO ReturnTickets (TicketID, LibrarianID, ReturnDate, FineAmount)
VALUES 
(1, 'LB001', '2022-01-23', 0),
(2, 'LB002', '2022-03-02', 5000),
(4, 'LB004', '2022-04-20', 5000),
(5, 'LB005', '2022-05-25', 0),
(6, 'LB001', '2022-07-03', 5000),
(8, 'LB003', '2022-09-12', 0),
(9, 'LB004', '2022-09-30', 5000),
(11, 'LB002', '2023-01-18', 0),
(12, 'LB003', '2023-03-01', 5000),
(14, 'LB005', '2023-05-10', 5000),
(15, 'LB001', '2023-06-12', 0),
(16, 'LB002', '2023-06-30', 5000),
(18, 'LB004', '2023-08-23', 0),
(19, 'LB005', '2023-09-20', 5000),
(21, 'LB003', '2024-01-21', 0),
(22, 'LB004', '2024-02-29', 5000),
(24, 'LB001', '2024-04-30', 5000),
(25, 'LB002', '2024-06-02', 0),
(26, 'LB003', '2024-07-10', 5000),
(28, 'LB005', '2024-08-28', 0),
(29, 'LB001', '2024-09-25', 5000),
(31, 'LB004', '2025-01-16', 0),
(32, 'LB005', '2025-02-22', 5000),
(34, 'LB002', '2025-05-03', 5000),
(35, 'LB003', '2025-06-05', 0),
(36, 'LB004', '2025-07-13', 5000),
(38, 'LB001', '2025-08-28', 0),
(39, 'LB002', '2025-10-05', 5000);
-- Update existing records with a default book (optional)
SELECT 
                        IDSTAFF AS ID, 
                        FULLNAME AS [FULL NAME], 
                        CONVERT(VARCHAR(10), DOB, 103) AS [D.O.B], 
                        EMAIL, 
                        PHONE, 
                        [ROLE], 
                        CASE WHEN STATUS = 1 THEN 'ON' ELSE 'OFF' END AS STATUS 
                      FROM LIBRARIANS 
                      ORDER BY [STATUS] DESC
SELECT 
    B.BOOKID AS [ID],
    B.TITLE,
    A.FULLNAME AS AUTHOR,
    P.NAME AS PUBLISHER,
    C.NAME AS CATEGORY,
    B.PUBLICATIONYEAR AS [YEAR],
    B.BORROWEDCOUNT AS [NO.BORROWED],
    B.DATEADDB AS [DATE ADD]
FROM 
    BOOKS B
    INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
    INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    B.STATUS = 1 
    AND NOT EXISTS (
        SELECT 1 
        FROM BORROWINGTICKETS BT 
        WHERE BT.BOOKID = B.BOOKID 
		AND  (BT.[STATUS] = N'Borrowing' OR BT.[APPROVAL_STATUS] = N'Waiting'));

	SELECT 
		R.READERID,
		R.FULLNAME AS [Reader Name],
		R.EMAIL,
		R.PHONE,
		R.[ADDRESS],
		R.DATEOFBIRTH,
		R.REGISTRATIONDATE,
		COUNT(BT.TICKETID) AS [Number of Borrowed Books]
	FROM 
		READERS R
		INNER JOIN BORROWINGTICKETS BT ON R.READERID = BT.READERID AND BT.[STATUS] = 'Borrowing'
	GROUP BY 
		R.READERID, R.FULLNAME, R.EMAIL, R.PHONE, R.[ADDRESS], R.DATEOFBIRTH, R.REGISTRATIONDATE;
       
SELECT 
    B.TITLE,
    A.FULLNAME AS [AUTHOR],
    C.NAME AS [CATEGORY],
    BT.BORROWDATE AS [BORROW DATE],
    BT.DUEDATE AS [DUE DATE],
    BT.RETURNDATE AS [RETURN DATE],
    CASE 
        WHEN BT.[STATUS] = 'Restored' THEN 'Restored'
        WHEN BT.[STATUS] = 'Borrowing' THEN 'Borrowing'
        ELSE BT.[STATUS] 
    END AS [STATUS],
    RT.FINEAMOUNT AS 'FINE AMOUNT'
FROM 
    BORROWINGTICKETS BT
    JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
    JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    LEFT JOIN RETURNTICKETS RT ON BT.TICKETID = RT.TICKETID
WHERE 
    BT.READERID = 4
ORDER BY 
    BT.BORROWDATE DESC;
SELECT 
    C.NAME AS CategoryName,
    COUNT(*) AS TotalReturns,
    SUM(CASE WHEN BT.RETURNDATE > BT.DUEDATE THEN 1 ELSE 0 END) AS OverdueCount,
    SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) AS TotalRestored,
    CASE 
        WHEN SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) > 0
        THEN SUM(CASE WHEN BT.RETURNDATE > BT.DUEDATE THEN 1 ELSE 0 END) * 100.0 / 
             SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END)
        ELSE 0 
    END AS OverdueRate
FROM 
    BORROWINGTICKETS BT
JOIN 
    BOOKS B ON BT.BOOKID = B.BOOKID
JOIN 
    CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    BT.BORROWDATE BETWEEN '2021-01-01' AND '2025-03-25' 
    AND BT.APPROVAL_STATUS = 'Approve'
    AND BT.STATUS = 'Restored' -- Chỉ tính các bản ghi đã trả
GROUP BY 
    C.NAME
ORDER BY 
    OverdueRate DESC

	SELECT 
    DATENAME(WEEKDAY, BT.BORROWDATE) AS DayOfWeek,
    COUNT(BT.TICKETID) AS LoanCount,
    COUNT(DISTINCT BT.READERID) AS UniqueReaders
FROM 
    BORROWINGTICKETS BT
WHERE 
    BT.BORROWDATE BETWEEN  '2021-01-01' AND '2025-03-25' 
    AND BT.APPROVAL_STATUS = 'Approve'
GROUP BY 
    DATENAME(WEEKDAY, BT.BORROWDATE),
    DATEPART(WEEKDAY, BT.BORROWDATE)
ORDER BY 
    DATEPART(WEEKDAY, BT.BORROWDATE)
-----------------
SELECT 
    B.TITLE AS [TITLE],
    R.FULLNAME AS [READER NAME],
    BT.DUEDATE AS [DUE DATE],
    DATEDIFF(day, BT.DUEDATE, GETDATE()) AS [DAY OVERDUE],
    CASE 
        WHEN DATEDIFF(day, BT.DUEDATE, GETDATE()) > 0 
        THEN DATEDIFF(day, BT.DUEDATE, GETDATE()) * 5000 
        ELSE 0 
    END AS [FINE AMOUNT]
FROM 
    BORROWINGTICKETS BT
    INNER JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    INNER JOIN READERS R ON BT.READERID = R.READERID
WHERE 
    BT.[STATUS] = 'Borrowing' 
    AND R.READERID=2; 