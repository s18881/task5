using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Tutorial3.Models;
using Tutorial3.Models.Enrollment;
using Tutorial3.Models.Promotion;
using Tutorial3.Services;

namespace Tutorial3.Controllers
{
    
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentController : ControllerBase
    {
        private IStudentDbService _service;
        public EnrollmentController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost(Name = nameof(EnrollStudent))]
        [Route("enroll")]
        [SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
        public IActionResult EnrollStudent(AddEnrollment student)
        {
            var result = _service.EnrollStudent(student);
            if (result.Studies != null) return CreatedAtAction(nameof(EnrollStudent), result);
            return BadRequest(result.IdStudent);
            /*var random = new Random();
            var idStudent = student.IdStudent;
            var firstName = student.FirstName;
            var lastName = student.LastName;
            var birthDate = student.BirthDate;
            var studies = student.Studies;

            var isNumeric = int.TryParse(idStudent, out _);

            if (string.IsNullOrEmpty(idStudent) || !isNumeric)
                return BadRequest("Invalid Data In The Field IdStudent");
            if (string.IsNullOrEmpty(firstName) || !Regex.Match(firstName,
                "^([A-Z])([a-z]{1,})$").Success)
                return BadRequest("Invalid Data In The Field FirstName");
            if (string.IsNullOrEmpty(lastName) || !Regex.Match(lastName,
                "^([A-Z])([a-z]{1,})$").Success)
                return BadRequest("Invalid Data In The Field LastName");
            if (string.IsNullOrEmpty(birthDate) || !Regex.Match(birthDate,
                @"^(0[1-9]|1[012])[\-](0[1-9]|[12][0-9]|3[01])[\-](19|20)\d\d$").Success)
                return BadRequest("Invalid Data In The Field BirthDate");
            if (string.IsNullOrEmpty(studies) || !Regex.Match(studies,
                "^([A-Z]{1})([A-Za-z]{1,})$").Success)
                return BadRequest("Invalid Data In The Field Studies");

            if (AddEnrollment.IsThereStudentWithId(idStudent))
                return BadRequest("Provided IdStudent Is Already In Use.");
            if (!AddEnrollment.IsThereStudy(studies))
                return BadRequest("There Is No Such Study");

            var createCustomEnrollment = !AddEnrollment.DoesStudyHaveSemesterOne(studies);
            var studyId = AddEnrollment.GetStudyId(studies);
            int customEnrollmentId;
            while (true)
            {
                customEnrollmentId = random.Next(0, 2000);
                if (!AddEnrollment.IsThereEnrollmentWithId(customEnrollmentId.ToString())) break;
            }
            var enrollmentId = AddEnrollment.GetEnrollmentIdWithSemOne(studyId);
            using (var sqlConnection =
                new SqlConnection(@"Server=db-mssql.pjwstk.edu.pl;Database=s18881;User Id=apbds18881;Password=admin;"))
            {
                sqlConnection.Open();
                var transaction = sqlConnection.BeginTransaction("SampleTransaction");
                if (createCustomEnrollment)
                {
                    using (var customEnrollmentCommand = new SqlCommand())
                    {
                        customEnrollmentCommand.Connection = sqlConnection;
                        customEnrollmentCommand.CommandText = "INSERT INTO Enrollment" +
                                                              "(IdEnrollment, Semester, IdStudy, StartDate) " +
                                                              "VALUES (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                        customEnrollmentCommand.Parameters.AddWithValue("idEnrollment", customEnrollmentId);
                        customEnrollmentCommand.Parameters.AddWithValue("Semester", 1);
                        customEnrollmentCommand.Parameters.AddWithValue("IdStudy", studyId);
                        customEnrollmentCommand.Parameters.AddWithValue("StartDate", DateTime.Now);
                        customEnrollmentCommand.Transaction = transaction;
                        try
                        {
                            customEnrollmentCommand.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Commit Exception Type: {0}", e.GetType());
                            Console.WriteLine("  Message: {0}", e.Message);
                            try
                            {
                                transaction.Rollback();
                                return Problem(e.Message);
                            }
                            catch (Exception e2)
                            {
                                Console.WriteLine("Rollback Exception Type: {0}", e2.GetType());
                                Console.WriteLine("  Message: {0}", e2.Message);
                                return Problem(e.Message);
                            }
                        }
                    }
                }

                using (var mainCommand = new SqlCommand())
                {
                    mainCommand.Connection = sqlConnection;
                    mainCommand.CommandText = "INSERT INTO Student " +
                                              "(IdStudent, FirstName, LastName, BirthDate, IdEnrollment) " +
                                              "VALUES (@IdStudent, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                    mainCommand.Parameters.AddWithValue("idStudent", idStudent);
                    mainCommand.Parameters.AddWithValue("FirstName", firstName);
                    mainCommand.Parameters.AddWithValue("LastName", lastName);
                    if (!DateTime.TryParseExact(birthDate, "MM-dd-yyyy", null, DateTimeStyles.None, out var simpleDate ))
                        return Problem("Problem with Parsing the Date");
                    mainCommand.Parameters.AddWithValue("BirthDate", simpleDate);
                    mainCommand.Parameters.AddWithValue("IdEnrollment", enrollmentId);
                    mainCommand.Transaction = transaction;
                    try
                    {
                        mainCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Commit Exception Type: {0}", e.GetType());
                        Console.WriteLine("  Message: {0}", e.Message);
                        try
                        {
                            transaction.Rollback();
                            return Problem(e.Message);
                        }
                        catch (Exception e2)
                        {
                            Console.WriteLine("Rollback Exception Type: {0}", e2.GetType());
                            Console.WriteLine("  Message: {0}", e2.Message);
                            return Problem(e.Message);
                        }
                    }
                }
                transaction.Commit();
            }

            var result = AddEnrollment.GetStudent(student.IdStudent);
            return CreatedAtAction(nameof(EnrollStudent), result);*/
        }


        [HttpGet("{idStudent}", Name = "StudentGetter")]
        public IActionResult GetStudent(string idStudent)
        {
            using var sqlConnection =
                new SqlConnection(@"Server=db-mssql.pjwstk.edu.pl;Database=s18881;User Id=apbds18881;Password=admin;");
            using var command = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = "SELECT  Semester " +
                              "FROM Student, Enrollment " +
                              "WHERE IdStudent=@idStudent " +
                              "AND Student.IdEnrollment = Enrollment.IdEnrollment"
            };
            SqlParameter parameter = new SqlParameter();
            command.Parameters.AddWithValue("idStudent", idStudent);
            sqlConnection.Open();
            SqlDataReader dataReader = command.ExecuteReader();
            while(dataReader.Read()) 
                return Ok("Student(" + idStudent + ") started his/her studies in " +
                          Int32.Parse(dataReader["Semester"].ToString()) + ".");
            return NotFound("Invalid Input Provided");
        }
        
        [HttpPost(Name = nameof(Promote))]
        [Route("promote")]
        public IActionResult Promote(PromoteStudents study)
        {
            /*var studies = study.Studies;
            var semester = study.Semester;
            var isNumeric = int.TryParse(semester, out _);
            
            if (string.IsNullOrEmpty(studies) || !Regex.Match(studies,
                "^([A-Z]{1})([A-Za-z]{1,})$").Success)
                return BadRequest("Invalid Data In The Field Studies");
            if (string.IsNullOrEmpty(semester) || !isNumeric)
                return BadRequest("Invalid Data In The Field IdStudent");
            
            if (!PromoteStudents.IsThereStudy(studies))
                return BadRequest("There Is No Such Study");

            using (var sqlConnection = new SqlConnection(@"Server=db-mssql.pjwstk.edu.pl;Database=s18881;User Id=apbds18881;Password=admin;"))
            {
                sqlConnection.Open();
                var transaction = sqlConnection.BeginTransaction("Transaction");
                using var mainCommand = new SqlCommand();
                mainCommand.Connection = sqlConnection;
                mainCommand.CommandText = "EXEC PromoteProcedure @Name, @Semester";
                mainCommand.Parameters.AddWithValue("Name", study.Studies);
                mainCommand.Parameters.AddWithValue("Semester", study.Semester);
                mainCommand.Transaction = transaction;
                mainCommand.ExecuteNonQuery();
                try
                {
                    mainCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Commit Exception Type: {0}", e.GetType());
                    Console.WriteLine("  Message: {0}", e.Message);
                    try
                    {
                        transaction.Rollback();
                        return Problem(e.Message);
                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine("Rollback Exception Type: {0}", e2.GetType());
                        Console.WriteLine("  Message: {0}", e2.Message);
                        return Problem(e.Message);
                    }
                }
                transaction.Commit();
            }
            var result = PromoteStudents.GetEnrollment(study.Studies);
            return CreatedAtAction(nameof(Promote), result);*/
            var result = _service.Promote(study);
            if (result.Semester != null) return CreatedAtAction(nameof(Promote), result);
            return BadRequest(result.Studies);
        }
    }
}
