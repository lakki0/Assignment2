using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private List<EmpModel> EmpList()
        {
            List<EmpModel> empModels = new List<EmpModel>();

            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("GetConn"));
            SqlCommand cmd = new SqlCommand("Select * from Employee",con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                EmpModel emp = new EmpModel();
                emp.Id = (int)dt.Rows[i]["Id"];
                emp.Name = dt.Rows[i]["Name"].ToString();
                emp.Email = dt.Rows[i]["Email"].ToString();
                emp.Role = dt.Rows[i]["Role"].ToString();
                emp.Password = dt.Rows[i]["Password"].ToString();

                empModels.Add(emp);
            }
            return empModels;
        }
        [HttpGet]
        [Route("GetAll")]
        public List<EmpModel> GetAll()
        {
            return EmpList();
        }


        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> Login([FromBody] EmpModel emplyoee)
        {
            EmpModel emp = new EmpModel();
            if (emplyoee == null)
            {
                return BadRequest();
            }

            var user = EmpList().Where(e => e.Email == emplyoee.Email && e.Password == emplyoee.Password).ToList();

            if (user == null)
            {

                return NotFound();
            }

           var token = CreateToken(user.FirstOrDefault());

            return Ok(new { user = user, token = token }); 


        }

        private string CreateToken(EmpModel emplyoee)
        {
            List<Claim> claims = new List<Claim>
             {
               new Claim(ClaimTypes.Name, emplyoee.Email),
               new Claim(ClaimTypes.Role, emplyoee.Role)
              };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

              _configuration["JWT:Issuer"],

              _configuration["JWT:Audience"],

              claims,

              expires: DateTime.Now.AddDays(10),

              signingCredentials: signIn);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Route("AddEmployee"), Authorize(Roles = "Admin")]
        public string AddEmployee(EmpModel emp)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("GetConn"));
            SqlCommand cmd = new SqlCommand("Insert into Employee values ('"+emp.Name+"','"+emp.Email+"','"+emp.Role+"','"+emp.Password+"'");
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            return "Employee Added Successfully";
        }

        [HttpGet]
        [Route("GetEmployee/{Id}")]
        public ActionResult<EmpModel> GetEmployeeById(int Id)
        {
            try
            {
                EmpModel result = EmpList().FirstOrDefault(x => x.Id == Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPut]
        [Route("UpdateEmployee"), Authorize(Roles = "Admin")]
        public string UpdateEmployee(EmpModel emp)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("GetConn"));
            SqlCommand cmd = new SqlCommand("Update Employee Set Name='"+emp.Name+"',Email= '"+emp.Email+"',Role= '"+emp.Password+"'" +"where Id = '"+emp.Id+"'" , con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return "Employee Updated Successfully";
        }

        [HttpDelete]
        [Route("DeleteEmplyee/{Id}"), Authorize(Roles = "Admin")]
        public string DeleteEmployee(int Id)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("GetConn"));
            SqlCommand cmd = new SqlCommand("Delete from Employee where Id= '" + Id + "'", con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return "Employee Delete Successfully";
        }

    }
    
}
