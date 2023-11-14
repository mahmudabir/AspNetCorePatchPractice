using Elfie.Serialization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace PatchPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetUsers()
        {
            if (_context.Persons == null)
            {
                return NotFound();
            }
            return await _context.Persons.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetUser(int id)
        {
            if (_context.Persons == null)
            {
                return NotFound();
            }
            var user = await _context.Persons.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // patch: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] PersonPatchVM model, bool allowNull = false)
        {
            try
            {
                JsonPatchDocument<Person> jsonPathUser = model.ToJsonPatchDocument<PersonPatchVM, Person>();

                if (!allowNull)
                {
                    jsonPathUser.Operations.RemoveAll(x => x.value == null);
                }

                //JsonPatchDocument<User> jsonPathUser2 = model.ToJsonPatchDocument(); // if model is type of User
                //return Ok(jsonPathUser);

                Person user = await _context.Persons.FindAsync(id);
                jsonPathUser.ApplyTo(user);

                _context.SaveChanges();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // patch: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("{id}/ByEntity")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] PersonPatchVM model)
        {
            //try
            //{
            Expression<Func<SetPropertyCalls<Person>, SetPropertyCalls<Person>>> setPropertyExpressionChain = Extension.GenerateSetPropertyExpressionChain<PersonPatchVM, Person>(model);
            _context.Persons.Where(x => x.Id == id).ExecuteUpdate(setPropertyExpressionChain);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok();
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}
        }

        // patch: api/Users/5/JsonPatch
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("{id}/JsonPatch")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] JsonPatchDocument<Person> model)
        {
            //return Ok(model);
            try
            {
                Person user = await _context.Persons.FindAsync(id);
                model.ApplyTo(user);
                _context.SaveChanges();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/DictionaryPatch")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] Dictionary<string, object> model)
        {
            //return Ok(model);
            try
            {
                JsonPatchDocument<Person> jsonPathUser = model.ToJsonPatchDocument<Person>();

                Person user = await _context.Persons.FindAsync(id);
                jsonPathUser.ApplyTo(user);
                _context.SaveChanges();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, Person user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            //_context.Entry(user).State = EntityState.Modified;

            _context.Persons.Where(x => x.Id == id).ExecuteUpdate(e => e
                                                    .SetProperty(x => x.Id, id)
                                                    .SetProperty(x => x.Name, "Abir Mahmud")
                                                    .SetProperty(x => x.Age, 26)
                                                    .SetProperty(x => x.Gender, "Male")
                                                    );

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw ex;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Person>> PostUser(Person user)
        {
            if (_context.Persons == null)
            {
                return Problem("Entity set 'AppDbContext.Users'  is null.");
            }
            _context.Persons.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Persons == null)
            {
                return NotFound();
            }
            var user = await _context.Persons.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Persons.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Persons?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
