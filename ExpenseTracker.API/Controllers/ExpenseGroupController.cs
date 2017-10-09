using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Http.Routing;
using System.Web;
using ExpenseTracker.API.Attributes;

namespace ExpenseTracker.API.Controllers
{
    [RoutePrefix("api")]
    public class ExpenseGroupController : ApiController
    {
        private const int maxPageSize = 2;
        //public IHttpActionResult Get()
        //{
        //    try
        //    {
        //        var expenseGroupRepository = new ExpenseRepository();
        //        var expenseGroups = expenseGroupRepository.Get();
        //        return Ok(expenseGroups);
        //    }
        //    catch (Exception)
        //    {
        //        return InternalServerError();
        //    }
        //}
            
            [Route("expenseGroups/get",Name = "ExpenseGroupList")]
        public IHttpActionResult Get(string sort="Id",string status=null, int page=1,int pageSize=5,string fields=null)
        {
            try
            {
                int statusId = -1;
                if (status != null)
                {
                    switch (status.ToLower())
                    {
                        case "open":
                            statusId=1 ;
                            break;
                        case "confiremd":
                            statusId = 2;
                            break;
                        case "processed":
                            statusId = 3;
                            break;
                        default:
                            break;
                    }
                }
                List<string> lstOfFields=new List<string>();

                if (fields != null)
                {
                    lstOfFields = fields.ToLower().Split(',').ToList();
                }
                    




                var expenseGroupRepository = new ExpenseRepository();
                var expenseGroups = expenseGroupRepository.Get()
                    .ApplySort(sort)
                    .Where(eg => (statusId == -1 || eg.StatusId == statusId))
                    .ToList()
                    .Select(exp => expenseGroupRepository.Get(exp, lstOfFields));

                if (pageSize > maxPageSize)
                {
                    pageSize = maxPageSize;
                }
                var totalCount = expenseGroups.Count();
                var totalpages = (int) Math.Ceiling((double) totalCount / pageSize);
                var urlHelper = new UrlHelper(Request);
                var prvLink = page > 1
                    ? urlHelper.Link("ExpenseGroupList", new
                    {
                        page = page - 1,
                        pageSize = pageSize,
                        sort = sort,
                        status = status,
                        fields=fields
                    })
                    : string.Empty;

                var nextLink = page < totalpages
                    ? urlHelper.Link("ExpenseGroupList", new
                    {
                        page = page + 1,
                        pageSize = pageSize,
                        sort = sort,
                        status = status,
                        fields = fields
                    })
                    : string.Empty;

                var pageHeader = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalpages = totalpages,
                    previousPageLink = prvLink,
                    NextPageLink = nextLink
                };

                HttpContext.Current.Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(pageHeader));




                return Ok(expenseGroups
                        .Skip(pageSize*(page-1))
                        .Take(pageSize)
                        .ToList()
                     
                    
                    );
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Route("expensegroup/{expenseGroupId}/expenses")]
        public IHttpActionResult Get(int expenseGroupId)
        {
            try
            {
                var expenseGroupRepository = new ExpenseRepository();
                var expenseGroup = expenseGroupRepository.Get(expenseGroupId);
                return Ok(expenseGroup.Expense);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [VersionRoute("expensegroup/{expenseGroupId}/expenses/{id}",1)]
        [VersionRoute("expenses/{id}",1)]
        public IHttpActionResult Getv1(int id,int? expenseGroupId=null)
        {
            try
            {
                ExpenseModel expense = null;
                var expenseGroupRepository = new ExpenseRepository();
                if (expenseGroupId == null)
                {
                    expense= expenseGroupRepository.GetExpense(id);
                }
                else
                {
                    expense = expenseGroupRepository.Get((int)expenseGroupId).Expense.FirstOrDefault(x=>x.Id==id);
                }
                return Ok(expense);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }


        [VersionRoute("expensegroup/{expenseGroupId}/expenses/{id}",2)]
        [VersionRoute("expenses/{id}",2)]
        public IHttpActionResult Getv2(int id, int? expenseGroupId = null)
        {
            try
            {
                ExpenseModel expense = null;
                var expenseGroupRepository = new ExpenseRepository();
                if (expenseGroupId == null)
                {
                    expense = expenseGroupRepository.GetExpense(id);
                }
                else
                {
                    expense = expenseGroupRepository.Get((int)expenseGroupId).Expense.FirstOrDefault(x => x.Id == id);
                }
                return Ok(expense);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }



        public IHttpActionResult Post([FromBody] ExpenseGroupModel expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }

                var expenseGroupRepository = new ExpenseRepository();
                var result = expenseGroupRepository.Add(expenseGroup);
                if (result.Status == ResultStatus.Created)
                {
                    return Created(Request.RequestUri + "/" + expenseGroup.Id.ToString(), result.Entity);
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }



        }

        public IHttpActionResult PUT(int id,[FromBody] ExpenseGroupModel expenseGroup)
        {
            try
            {
                if (expenseGroup == null)
                {
                    return BadRequest();
                }
                var expenseGroupRepository = new ExpenseRepository();
                var result = expenseGroupRepository.Update(id, expenseGroup);
                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound();
                }
                if (result.Status == ResultStatus.Updated)
                {
                    return Ok(result.Entity);
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }



        }

        public IHttpActionResult Delete(int id)
        {
            try
            {
                var expenseGroupRepository = new ExpenseRepository();
                var result = expenseGroupRepository.Delete(id);
                if (result.Status == ResultStatus.Deleted)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound();
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

    }


    public class ExpenseGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public  int StatusId { get; set; }
        public List<ExpenseModel> Expense { get; set; }
    }

    public class ExpenseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public int ExpenseGroupId { get; set; }
    }

    public class ExpenseRepository
    {
        private List<ExpenseGroupModel> ExpenseGroups { get; set; }
        private List<ExpenseModel> Expense { get; set; }
        public ExpenseRepository()
        {

            ExpenseGroups = new List<ExpenseGroupModel>()
            {
                new ExpenseGroupModel {Id = 4, Name = "AExpenseGroup1",StatusId=1, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 1 ,Name = "Expense1" } } },
                new ExpenseGroupModel {Id = 3, Name = "BExpenseGroup2",StatusId=2, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 2 ,Name = "Expense2" } } },
                new ExpenseGroupModel {Id = 2, Name = "CExpenseGroup3",StatusId=3, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 3 ,Name = "Expense3" } } },
                new ExpenseGroupModel {Id = 1, Name = "DExpenseGroup4",StatusId=1, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 4 ,Name = "Expense4" } } },
                new ExpenseGroupModel {Id = 5, Name = "AExpenseGroup5",StatusId=2, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 5 ,Name = "Expense5" } } },
                new ExpenseGroupModel {Id = 6, Name = "BExpenseGroup6",StatusId=3, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 6 ,Name = "Expense6" } } },
                new ExpenseGroupModel {Id = 7, Name = "CExpenseGroup7",StatusId=1, Expense = new List<ExpenseModel> {new ExpenseModel {Id = 7 ,Name = "Expense7" } } },

            };

            Expense = new List<ExpenseModel>
            {
                new ExpenseModel
                {
                    Id = 1,
                    Name = "Expense1",
                    ExpenseGroupId = 1,
                },
                new ExpenseModel
                {
                    Id = 2,
                    Name = "Expense2",
                    ExpenseGroupId = 2,
                },
                new ExpenseModel
                {
                    Id = 3,
                    Name = "Expense3",
                    ExpenseGroupId = 3,
                },
                new ExpenseModel
                {
                    Id = 4,
                    Name = "Expense4",
                    ExpenseGroupId = 4,
                }
            };
        }
        public List<ExpenseGroupModel> Get()
        {
            return ExpenseGroups;
        }
        public ExpenseGroupModel Get(int id)
        {
            return ExpenseGroups.Where(x=>x.Id==id).FirstOrDefault();
        }

        public object Get(ExpenseGroupModel expenceGroup,List<string> lstOfFields)
        {
            if (!lstOfFields.Any())
            {
                return expenceGroup;
            }
            else
            {
                 ExpandoObject objectToReturn=new ExpandoObject();
                foreach (var field in lstOfFields)
                {
                    var fieldValue = expenceGroup.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(expenceGroup, null);

                    ((IDictionary<string,object>) objectToReturn).Add(field,fieldValue);
                }
                return objectToReturn;
            }
        }



        public DBResult<ExpenseGroupModel> Add(ExpenseGroupModel expenseGroup)
        {
            ExpenseGroups.Add(expenseGroup);
            var result = new DBResult<ExpenseGroupModel>()
            {
                Status = ResultStatus.Created,
                Entity = expenseGroup
            };
            return result;
        }
        public DBResult<ExpenseGroupModel> Update(int id,ExpenseGroupModel expenseGroup)
        {
            var savedexpenseGroup = ExpenseGroups.Where(g => g.Id == id).FirstOrDefault();
            if (savedexpenseGroup == null)
            {
               return  new DBResult<ExpenseGroupModel>()
                {
                    Status = ResultStatus.NotFound,
                    Entity = null
                };
            }
            savedexpenseGroup.Name = expenseGroup.Name;
            return new DBResult<ExpenseGroupModel>()
            {
                Status = ResultStatus.Updated,
                Entity = savedexpenseGroup
            };
        }
        public DBResult<ExpenseGroupModel> Delete(int id)
        {
            var expenseGroup = ExpenseGroups.FirstOrDefault(g => g.Id == id);
            if (expenseGroup == null)
            {
                return new DBResult<ExpenseGroupModel>()
                {
                    Status = ResultStatus.NotFound,
                    Entity = null
                };
            }
            ExpenseGroups.Remove(ExpenseGroups.First(g => g.Id == id));
            return new DBResult<ExpenseGroupModel>()
            {
                Status = ResultStatus.Deleted,
                Entity = expenseGroup
            };
        }
        public List<ExpenseModel> GetExpense()
        {
            return Expense;
        }
        public ExpenseModel GetExpense(int id)
        {
            return Expense.FirstOrDefault(x => x.Id == id);
        }
    }

    public class DBResult<T>
    {
        public ResultStatus Status { get; set; }
        public T Entity { get; set; }
    }

    public enum ResultStatus
    {
        Created,
        Updated,
        NotFound,
        Deleted
    }

    public static class IListExtentions
    {
        public static IList<T> ApplySort<T>(this IList<T> source, string sort)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (sort == null)
            {
                return source;
            }
            //split sort string
            var lstSort = sort.Split(',');

            string completeSortExpression = "";
            foreach (var sortOptions in lstSort)
            {
                if (sortOptions.StartsWith("-"))
                {
                    completeSortExpression = completeSortExpression + sortOptions.Remove(0, 1) + " descending,";
                }
                else
                {
                    completeSortExpression = completeSortExpression + sortOptions + ",";
                }
            }

            if (!string.IsNullOrWhiteSpace(completeSortExpression))
            {
                source = source.OrderBy(completeSortExpression.Remove(completeSortExpression.Length - 1)).ToList();
            }
            return source;
        }
    }




}
