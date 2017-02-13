BEGIN TRANSACTION;

INSERT INTO Tag VALUES (1,'Test');
INSERT INTO PostTag VALUES (2,1,1);
INSERT INTO PostContent VALUES (1,'## Test post

**Do not publish this post**',1);
INSERT INTO Post VALUES (1,'Matteo','2017-02-13 21:13:09.919654','This is a test post inserted using DbUp',NULL,'This is a test post');
INSERT INTO Comment VALUES (1,'Awesome blog :-)','2017-02-13 21:14:02.0778465',NULL,'Matteo',1);

COMMIT;
