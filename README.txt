DentalClinicWebSystemSingle (ASP.NET Core MVC, .NET 7)

Cum rulezi:
1) Deschide DentalClinicWebSystemSingle.sln in Visual Studio 2022
2) Set as Startup Project: DentalClinic.Web
3) Apasa F5

Login:
- email:admin@clinic.local
-  user: admin
-  pass: Admin123!

Baza de date:
- SQLite: dentalclinic.db (se creeaza automat la prima rulare in folderul proiectului)

CRUD:
- Doctors, Patients, Treatments, Appointments, Reviews (toate au Create/Read/Update/Delete)
- Relatii:
  Appointment -> Patient, Doctor, Treatment
  Review -> Appointment (1:1, un review per programare)

Autorizare:
- Toate controllerele sunt protejate cu [Authorize]
- CRUD: Admin/Staff
