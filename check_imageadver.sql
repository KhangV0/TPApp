-- Search ALL records for LIVESTREAM (case insensitive, wider search)
SELECT ID, Title, Subject, StatusID, Sort FROM ImagesAdver 
WHERE UPPER(Title) LIKE N'%LIVE%' OR Title LIKE N'%live%';

-- Full details of the 6 target ads
SELECT ID, Title, Description, SRC, URL, Subject, StatusID, Sort, Created, Creator
FROM ImagesAdver 
WHERE ID IN (80, 70, 64, 39, 62, 54)
ORDER BY Sort;
