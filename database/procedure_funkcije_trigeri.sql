USE CourseSystemDB;
GO

-- Triger 1 (Payment -> Enrollment -> Notification)
CREATE TRIGGER trg_AfterPayment
ON Payments
AFTER INSERT
AS
BEGIN
    INSERT INTO Enrollments (UserID, CourseID, Status)
    SELECT i.UserID, i.CourseID, 'Active'
    FROM inserted i;

    INSERT INTO Notifications (Message, UserID)
    SELECT 'Payment successful! You are now enrolled in course.', i.UserID
    FROM inserted i;
END
GO

-- Triger 2 (Results -> Enrollment Status)
CREATE TRIGGER trg_AfterResult
ON Results
AFTER INSERT
AS
BEGIN
    -- Notification
    INSERT INTO Notifications (Message, UserID)
    SELECT 
        CASE 
            WHEN i.Score >= 50 THEN 'You passed an exam!'
            ELSE 'You failed an exam.'
        END,
        i.UserID
    FROM inserted i;

    -- COMPLETE COURSE LOGIC
    UPDATE e
    SET Status = 'Completed'
    FROM Enrollments e
    JOIN Exams ex ON e.CourseID = ex.CourseID
    JOIN inserted i ON i.ExamID = ex.ExamID
    WHERE i.Score >= 50
    AND e.UserID = i.UserID;
END
GO

-- Procedura 1 (Add result)
CREATE PROCEDURE sp_AddResult
    @UserID INT,
    @ExamID INT,
    @Score INT
AS
BEGIN
    INSERT INTO Results (UserID, ExamID, Score)
    VALUES (@UserID, @ExamID, @Score);
END
GO

-- Procedura 2 ()


-- Funkcija 1 (da li je course completed)
CREATE FUNCTION fn_IsCourseCompleted
(
    @UserID INT,
    @CourseID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @Result BIT = 0;

    IF NOT EXISTS (
        SELECT 1
        FROM Exams e
        LEFT JOIN Results r 
            ON e.ExamID = r.ExamID 
            AND r.UserID = @UserID
        WHERE e.CourseID = @CourseID
        AND (r.Score IS NULL OR r.Score < 50)
    )
    SET @Result = 1;

    RETURN @Result;
END
GO

-- Funkcija 2 (average score usera)
CREATE FUNCTION fn_GetAverageScore(@UserID INT)
RETURNS FLOAT
AS
BEGIN
    DECLARE @Avg FLOAT;

    SELECT @Avg = AVG(CAST(Score AS FLOAT))
    FROM Results
    WHERE UserID = @UserID;

    RETURN ISNULL(@Avg, 0);
END
GO