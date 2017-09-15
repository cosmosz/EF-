using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Common;

namespace EntityFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                var query = from rv in context.Students
                            join rv1 in context.Standards
                            on rv.StandardId equals rv1.StandardId
                            select new { rv.StudentID, rv.StudentName, rv.StudentAddress, rv1.StandardName };
                var result = query.ToList();

                string sqlstring = query.ToString();
            }

            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                var L2Q = context.Students.Where(x => x.StudentName == "Peter");
            }

            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                //正确写法，返回Student实体类型
                var query = context.Students.SqlQuery("select * from Students").ToList();

                //错误写法，SQL查询返回的列应该匹配实体类型的属性，否则会抛出异常
                //var query1 = context.Students.SqlQuery("select StudentID from Students").ToList();

                //错误写法，更改查询项的列名称，那么它将抛出异常，因为它必须实体属性
                //var query2 = context.Students.SqlQuery("select StudentID as id, StudentName from Students").ToList();
            }

            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                //使用Database类上的SqlQuery方法创建返回任何类型的实例（包括基本类型）的SQL查询
                var query = context.Database.SqlQuery<string>("select StudentName from Students").FirstOrDefault();
            }

            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                //ExecuteSqlCommnad方法在将非查询命令发送到数据库（例如插入，更新或删除命令）时可用
                var quary1 = context.Database.ExecuteSqlCommand("delete from Students where StudentID = @p0", "1");
            }

            //using (SchoolDBEntities context = new SchoolDBEntities())
            //{
            //    var student = context.Students.Find("001");

            //    DbEntityEntry Entry = context.Entry(student);

            //    var state = Entry.State; //Unchanged

            //    student.StudentName = "killer";
            //    state = Entry.State; //Modified

            //    context.Students.Remove(student);
            //    state = Entry.State; //Deleted

            //    var stu = new Student { StudentID = "007", StudentName = "ppp" };
            //    state = context.Entry(stu).State; //Detached

            //    context.Students.Add(stu);
            //    state = context.Entry(stu).State; //Added

            //    //context.SaveChanges();  //将所有实体操作保存到数据库
            //}

            //using (SchoolDBEntities context = new SchoolDBEntities())
            //{
            //    var stu = new Student { StudentID = "007", StudentName = "ppp" };

            //    context.Students.Add(stu);

            //    context.SaveChanges();  //将所有实体操作保存到数据库
            //}

            //var stu = new Student { StudentID = "007", StudentName = "ppp" };
            //using (SchoolDBEntities context = new SchoolDBEntities())
            //{
            //    context.Entry(stu).State = System.Data.Entity.EntityState.Added;
            //    context.Students.Add(stu);

            //    context.SaveChanges();  //将所有实体操作保存到数据库
            //}

            //Student student;
            //using (SchoolDBEntities context = new SchoolDBEntities())
            //{
            //    student = context.Students.Find("001");
            //}
            //using (SchoolDBEntities newcontext = new SchoolDBEntities())
            //{
            //    //设置该对象的状态为Modified
            //    newcontext.Entry(student).State = System.Data.Entity.EntityState.Deleted;
            //    newcontext.SaveChanges();  //将所有实体操作保存到数据库
            //}


            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                //exec [dbo].[GetStudentByStudentId] @StudentId="002"
                var student = context.GetStudentByStudentId("002"); 
                foreach (GetStudentByStudentId_Result cs in student)
                {
                    //Console.WriteLine(cs.StudentName);
                }
            }

            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                var student = context.Students.Find("001");
                student.StudentName = "SSS";
                //exec [dbo].[sp_UpdateStudent] @StudentId='001',@StudentName='SSS'
                context.SaveChanges();
            }

            using (var ctx = new SchoolDBEntities())
            {
                //只加载Student表数据
                IList<Student> studList = ctx.Students.ToList<Student>();
                Student std = studList[0];
                //延迟加载与Student相关联的Address实体数据
                StudentAddress add = std.StudentAddress;
            }

            //LINQ查询语法
            using (var context = new SchoolDBEntities())
            {
                var res = (from s in context.Students.Include("StudentAddress")
                           where s.StudentName == "Student1"
                           select s).FirstOrDefault<Student>();
            }

            //LINQ方法语法
            using (var ctx = new SchoolDBEntities())
            {
                var stud = ctx.Students.Include("StudentAddress")
                                   .Where(s => s.StudentName == "Student1").FirstOrDefault<Student>();

            }

            //Lambda表达式（需要添加System.Data.Entity命名空间）
            using (SchoolDBEntities ctx = new SchoolDBEntities())
            {
                var stud = ctx.Students.Include(s => s.StudentAddress)
                                    .Where(s => s.StudentName == "Student1")
                                    .FirstOrDefault<Student>();

            }

            //DBCommandLogging();

            using (SchoolDBEntities context = new SchoolDBEntities())
            {
                context.Database.Log = new Action<string>(x => Console.Write(x));
                using (System.Data.Entity.DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        var student = context.Students
                                 .Where(s => s.StudentID == "001").FirstOrDefault<Student>();
                        context.Database.ExecuteSqlCommand(
                            @"UPDATE Students SET StudentName = 'sss1'" +
                                " WHERE StudentID ='001'"
                            );
                        //context.Students.Remove(std1);
                        context.SaveChanges();
                        //提交事务
                        dbTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        //回滚
                        dbTran.Rollback();
                    }
                }
            }
                

            Console.ReadLine();
        }

        public static void DBCommandLogging()
        {
            using (var context = new SchoolDBEntities())
            {
                //Context.Database.Log是一个Action <string>，可以附加任何具有一个字符串参数和void返回类型的方法
                context.Database.Log = new Action<string>(x => Console.Write(x));
                var student = context.Students
                                .Where(s => s.StudentID == "001").FirstOrDefault<Student>();
                student.StudentName = "SSS1";
                context.SaveChanges();
            }
        }


        //标记为async以使其异步，异步方法的返回类型必须是Task
        //GetStudent返回学生实体的对象，所以返回类型必须是Task<Student>
        private static async Task<Student> GetStudent()
        {
            Student myStudent = null;
            using (var context = new SchoolDBEntities())
            {
                Console.WriteLine("Start GetStudent...");

                //查询标有await。 这使得调用线程能够执行其他操作，直到它执行查询并返回数据。
                myStudent = await (context.Students.Where(s => s.StudentID == "001")
                    .FirstOrDefaultAsync<Student>());

                Console.WriteLine("Finished GetStudent...");
            }
            return myStudent;
        }


        //异步调用context.SaveChanges，与异步查询相同
        private static async Task SaveStudent(Student editedStudent)
        {
            using (var context = new SchoolDBEntities())
            {
                //Student对象以参数形式带入，断开连接下的实体操作
                context.Entry(editedStudent).State = EntityState.Modified;

                Console.WriteLine("Start SaveStudent...");

                int x = await (context.SaveChangesAsync());

                Console.WriteLine("Finished SaveStudent...");
            }
        }

        public static void AsyncQueryAndSave()
        {
            //调用异步查询
            var queryResult = GetStudent();
            Console.WriteLine("Let's do something else till we get student..");
            //等待异步查询
            queryResult.Wait();
            var student1 = queryResult.Result;
            student1.StudentName = "Modified First Name";
            //调用异步保存
            var studentSave = SaveStudent(student1);
            Console.WriteLine("Let's do something else till we save student..");
            //等待异步保存
            studentSave.Wait();
        }

       
    }
    class EFCommandInterceptor : IDbCommandInterceptor
    {
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            throw new NotImplementedException();
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            throw new NotImplementedException();
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            throw new NotImplementedException();
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            throw new NotImplementedException();
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            throw new NotImplementedException();
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            throw new NotImplementedException();
        }
    }
}
