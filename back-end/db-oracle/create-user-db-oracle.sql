-- Tạo user cho Identity service
create user hotelbooking_identity identified by "Ab@123456";
grant connect,resource to hotelbooking_identity;
grant create session,
   create table,
   create view,
   create sequence,
   create procedure
to hotelbooking_identity;
alter user hotelbooking_identity
   quota unlimited on users;

-- Tạo user cho Hotel service
create user hotelbooking_hotel identified by "Ab@123456";
grant connect,resource to hotelbooking_hotel;
grant create session,
   create table,
   create view,
   create sequence,
   create procedure
to hotelbooking_hotel;
alter user hotelbooking_hotel
   quota unlimited on users;

-- Tạo user cho Booking service
create user hotelbooking_booking identified by "Ab@123456";
grant connect,resource to hotelbooking_booking;
grant create session,
   create table,
   create view,
   create sequence,
   create procedure
to hotelbooking_booking;
alter user hotelbooking_booking
   quota unlimited on users;

-- Tạo user cho Payment service
create user hotelbooking_payment identified by "Ab@123456";
grant connect,resource to hotelbooking_payment;
grant create session,
   create table,
   create view,
   create sequence,
   create procedure
to hotelbooking_payment;
alter user hotelbooking_payment
   quota unlimited on users;

-- Tạo user cho Notification service
create user hotelbooking_notification identified by "Ab@123456";
grant connect,resource to hotelbooking_notification;
grant create session,
   create table,
   create view,
   create sequence,
   create procedure
to hotelbooking_notification;
alter user hotelbooking_notification
   quota unlimited on users;