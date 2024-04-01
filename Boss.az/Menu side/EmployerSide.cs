﻿using Boss.az.ConsoleMenuHelper;
using Boss.az.DatabaseNS;
using Boss.az.ExceptionNS;
using Boss.az.HelpersNS;
using Boss.az.HumanNS;
using Boss.az.Json;
using Boss.az.NetworkNS;
using Boss.az.Other;
using Boss.az.PostNS;
using ExtensionNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Boss.az.Menu_side
{
    static class EmployerSide
    {
        public static void EmployerMenu(Database db, Employer emp)
        {
            if (emp == null) throw new EmployerInfoException("Employer is null!");
            else if (db == null) throw new DatabaseInfoException("Database is null!");
            else
            {
            EmployerMenu:
                int choise = ConsoleHelper.MultipleChoice(5, 1, 1, true, "CV's", "Vacancy section", "Notifications", "Back to");
                switch (choise)
                {
                    case 0:
                        {
                        SubEmpMenu:
                            choise = ConsoleHelper.MultipleChoice(5, 1, 1, true, "Search", "All CV's", "Back to");
                            Console.Clear();
                            if (choise == 0)
                            {
                                if (db.Workers == null)
                                {
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.DarkGray;
                                    Console.WriteLine("There are currently no workers and no CV's!");
                                    Console.Write("Click any to go back...");
                                    Console.ResetColor();
                                    Console.ReadLine();
                                    goto SubEmpMenu;
                                }
                                else
                                {
                                    uint filterCatId;
                                    var array = new String[db.Categories.ToList().Count];
                                    for (int i = 0; i < db.Categories.ToList().Count; i++)
                                        array[i] = db.Categories.ToArray()[i].Name.FirstCharToUpper();
                                    Console.Write("Category: ");
                                    choise = ConsoleHelper.MultipleChoice(5, 1, 3, false, array);
                                    filterCatId = db.Categories.ToList().Find(c => c.Name == array[choise]).Id;
                                    array = new String[db.SubCategories.Count(s => s.CategoryId == filterCatId)];
                                    for (int i = 0; i < db.SubCategories.Count(s => s.CategoryId == filterCatId); i++)
                                        array[i] = db.SubCategories.ToList().FindAll(s => s.CategoryId == filterCatId).ToArray()[i].Name;
                                    Console.Clear();
                                    Console.Write("Subcategory: ");
                                    choise = ConsoleHelper.MultipleChoice(5, 1, 1, false, array);
                                    uint filterSubCatId = db.SubCategories.ToList().Find(s => s.Name == array[choise]).Id;
                                    List<Cv> cvs = new();
                                    List<string> arr = new();
                                    array = Array.Empty<string>();
                                    for (int i = 0; i < db.Workers.Count; i++)
                                    {
                                        for (int j = 0; j < db.Workers[i].Cvs.Count; j++)
                                        {
                                            arr.Add(db.Workers[i].Cvs[j].WorkCity.Trim().ToLower().FirstCharToUpper());
                                            cvs.Add(db.Workers[i].Cvs[j]);
                                        }
                                    }
                                    array = arr.ToArray();
                                    array = arr.Distinct<string>().ToArray();
                                    array.ToList().TrimExcess();
                                    Console.Clear();
                                    Console.Write("City: ");
                                    choise = ConsoleHelper.MultipleChoice(5, 1, 3, false, array);
                                    string filterCity = cvs.Find(v => v.WorkCity == array[choise]).WorkCity;
                                    Console.Clear();
                                    Console.Write("Enter max salary: ");
                                    double filterMaxSalary = double.Parse(Console.ReadLine());
                                    Console.Clear();
                                    Console.WriteLine("Choise experience");
                                    string[] experienceChoiseArray = { "No", "Less than 1 year", "From 1 to 3 years", "From 3 to 5 years", "More than 5 years" };
                                    choise = ConsoleHelper.MultipleChoice(26, 1, 1, false, experienceChoiseArray);
                                    string filterExperience = choise switch
                                    {
                                        0 => experienceChoiseArray[choise],
                                        1 => experienceChoiseArray[choise],
                                        2 => experienceChoiseArray[choise],
                                        3 => experienceChoiseArray[choise],
                                        4 => experienceChoiseArray[choise],
                                        _ => "None"
                                    };
                                RetryCvChoise:
                                    Console.Clear();
                                    if (cvs.Exists(c => c.CategoryId == filterCatId && c.SubCategoryId == filterSubCatId && c.WorkCity.Trim().ToLower() == filterCity.Trim()
                                        .ToLower() && c.Salary <= filterMaxSalary && c.Experience.Trim().ToLower() == filterExperience.Trim().ToLower()))
                                    {
                                        cvs.FindAll(c => c.CategoryId == filterCatId
                                                               && c.SubCategoryId == filterSubCatId
                                                               && c.WorkCity.Trim().ToLower() == filterCity.Trim().ToLower()
                                                               && c.Salary <= filterMaxSalary
                                                               && c.Experience.Trim().ToLower() == filterExperience.Trim().ToLower()).ForEach(c =>
                                                               {
                                                                   Console.WriteLine("\n-----------------------------------------");
                                                                   Console.ForegroundColor = ConsoleColor.DarkGreen;
                                                                   Console.WriteLine($"Cv guid: {c.Guid}");
                                                                   Console.ResetColor();
                                                                   ++c.ViewCount;
                                                                   db.Workers.Find(w => w.Cvs.Exists(c => c.Guid == c.Guid)).Show();
                                                                   Console.WriteLine("-------- CV info --------");
                                                                   c.Show(db);
                                                                   Console.WriteLine("----------------------------");
                                                               });
                                        choise = ConsoleHelper.MultipleChoice(1, 0, 2, false, "Follow", "Back to");
                                        if (choise == 0)
                                        {
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            Console.SetCursorPosition(52, 0);
                                            Console.WriteLine("(If you want to go back, type >> back <<)");
                                            Console.ResetColor();
                                            Console.SetCursorPosition(52, 1);
                                            Console.Write("Enter follow guid: ");
                                            string guid = Console.ReadLine();
                                            if (guid.Trim().ToLower() == "back") goto SubEmpMenu;
                                            else if (db.Workers.ToList().Exists(e => e.Cvs.Exists(v => v.Guid.ToString() == guid)))
                                            {
                                                Console.Clear();
                                                Worker worker = new();
                                                worker = db.Workers.ToList().Find(e => e.Cvs.Exists(v => v.Guid.ToString() == guid));
                                                worker.Notifications = new();
                                                worker.IncomingVacancies = new();
                                                emp.ShowAllVacancy(db);
                                                if (emp.Vacancies != null)
                                                {
                                                    Console.BackgroundColor = ConsoleColor.Gray;
                                                    Console.ForegroundColor = ConsoleColor.Black;
                                                    Console.WriteLine("(If you want to go back, type >> back <<)");
                                                    Console.ResetColor();
                                                    Console.Write("Enter the vacancy id you want to send to the worker: ");
                                                    guid = Console.ReadLine();
                                                    if (guid.Trim().ToLower() == "back") goto RetryCvChoise;
                                                    else if (emp.Vacancies.Exists(c => c.Guid.ToString() == guid))
                                                    {
                                                        Notification notf = new() { Title = "New Vakansiya!", Text = "Your CV has been reviewed. I offer you a job!", FromUser = emp.Name + " " + emp.Surname, PostGuid = guid };
                                                        Console.WriteLine($"Your vacancy has been sent to {worker.Name} {worker.Surname}!");
                                                        worker.IncomingVacancies.Add(emp.Vacancies.Find(c => c.Guid.ToString() == guid));
                                                        worker.Notifications.Add(notf);
                                                        FileProcessJson.JsonFileWriteAllText("Workers.json", db.Workers);
                                                        Thread.Sleep(2000);
                                                        goto SubEmpMenu;
                                                    }
                                                    else
                                                    {
                                                        Console.Clear();
                                                        Console.ForegroundColor = ConsoleColor.DarkRed;
                                                        Console.WriteLine("No vacancies were found matching the guid you entered!");
                                                        Console.ResetColor();
                                                        Thread.Sleep(1000);
                                                        Console.ReadLine();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                                Console.WriteLine("No matching CV found in this guid!");
                                                Console.ResetColor();
                                                Thread.Sleep(1500);
                                                goto SubEmpMenu;

                                            }
                                        }
                                        else if (choise == 1)
                                            goto SubEmpMenu;
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.DarkRed;
                                        Console.WriteLine("Nothing was found!");
                                        Thread.Sleep(1500);
                                        goto SubEmpMenu;
                                    }
                                    FileProcessJson.JsonFileWriteAllText<List<Employer>>("Employers.json", db.Employers);
                                }
                            }
                            else if (choise == 1)
                            {
                                if (db.Workers == null)
                                {
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                    Console.WriteLine("There are currently no worker and CV!");
                                    Thread.Sleep(1500);
                                    goto SubEmpMenu;
                                }
                                else
                                {
                                    foreach (var worker in db.Workers)
                                    {
                                        if (worker.Cvs == null)
                                        {
                                            Console.Clear();
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("There are currently no CVs!");
                                            Console.ResetColor();
                                            Thread.Sleep(1500);
                                            goto SubEmpMenu;
                                        }
                                        else
                                        {
                                            Console.Clear();
                                            Console.WriteLine("\t\t-------------- List of workers -------------- ");
                                            db.Workers.ForEach(w =>
                                            {
                                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                                Console.WriteLine($"Guid: {w.Guid}");
                                                Console.ResetColor();
                                                w.Show();
                                                Console.WriteLine();
                                            });
                                        RetryGuid:
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                            Console.ForegroundColor = ConsoleColor.Black;
                                            Console.WriteLine("(If you want to go back, type >> back <<)");
                                            Console.ResetColor();
                                            Console.Write("Enter the guide of the worker you want to look at in your CV: ");
                                            string guid = Console.ReadLine();
                                            if (guid.Trim().ToLower() == "back") goto SubEmpMenu;
                                            if (!Helpers.IsValidEmpty(guid))
                                                goto RetryGuid;
                                            if (db.Workers.Exists(w => w.Guid.ToString() == guid))
                                            {
                                                Console.Clear();
                                                Worker tempWorker = db.Workers.Find(w => w.Guid.ToString() == guid);
                                            TryView:
                                                foreach (var cv in tempWorker.Cvs)
                                                {
                                                    Console.Clear();
                                                    Console.WriteLine("\n----------------------------------------------");
                                                    ++cv.ViewCount;
                                                    FileProcessJson.JsonFileWriteAllText("Workers.json", db.Workers);
                                                    cv.ShowShort(db, tempWorker);
                                                    choise = ConsoleHelper.MultipleChoice(0, 0, 3, false, "Read more", "Next cv", "Back to menu");
                                                    switch (choise)
                                                    {
                                                        case 0:
                                                            {
                                                            TryView2:
                                                                Console.Clear();
                                                                Console.WriteLine("\n----------------------------------------------------");
                                                                ++cv.ViewCount;
                                                                FileProcessJson.JsonFileWriteAllText("Workers.json", db.Workers);
                                                                cv.Show(db);
                                                                choise = ConsoleHelper.MultipleChoice(1, 0, 2, false, "Follow", "Back to");
                                                                if (choise == 0)
                                                                {
                                                                    Console.Clear();
                                                                    tempWorker.Notifications = new();
                                                                    tempWorker.IncomingVacancies = new();
                                                                    emp.ShowAllVacancy(db);
                                                                    if (emp.Vacancies != null)
                                                                    {
                                                                    RetrySendVacancy:
                                                                        Console.BackgroundColor = ConsoleColor.Gray;
                                                                        Console.ForegroundColor = ConsoleColor.Black;
                                                                        Console.WriteLine("(If you want to go back, type >> back <<)");
                                                                        Console.ResetColor();
                                                                        Console.Write("Enter the guid for the vacancy you want to send to Worker: ");
                                                                        guid = Console.ReadLine();
                                                                        if (guid.Trim().ToLower() == "back")
                                                                            goto TryView2;
                                                                        if (!Helpers.IsValidEmpty(guid))
                                                                            goto RetrySendVacancy;
                                                                        if (emp.Vacancies.Exists(c => c.Guid.ToString() == guid))
                                                                        {
                                                                            Notification notf = new() { Title = "New Vakansiya!", Text = "Your resume has been reviewed. I offer you a job!", FromUser = emp.Name + emp.Surname, PostGuid = guid };
                                                                            Console.WriteLine($"Your vacancy has been sent to {tempWorker.Name} {tempWorker.Surname}!");
                                                                            tempWorker.IncomingVacancies.Add(emp.Vacancies.Find(c => c.Guid.ToString() == guid));
                                                                            tempWorker.Notifications.Add(notf);
                                                                            FileProcessJson.JsonFileWriteAllText("Workers.json", db.Workers);
                                                                            Thread.Sleep(1500);
                                                                            goto EmployerMenu;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        Console.ForegroundColor = ConsoleColor.DarkRed;
                                                                        Console.WriteLine("There is no vacancy for you!");
                                                                        Console.ResetColor();
                                                                        Thread.Sleep(1500);
                                                                    }
                                                                }
                                                                else continue;
                                                                break;
                                                            }
                                                        case 1: continue;
                                                        case 2: goto EmployerMenu;
                                                    }
                                                }
                                                Console.Clear();
                                                Console.WriteLine(">>> Butun cvlere baxdiniz <<<");
                                                choise = ConsoleHelper.MultipleChoice(1, 1, 1, false, "Yeniden baxmaq", "Back to");
                                                FileProcessJson.JsonFileWriteAllText("Workers.json", db.Workers);
                                                switch (choise)
                                                {
                                                    case 0: goto TryView;
                                                    case 1: goto EmployerMenu;
                                                }
                                                goto EmployerMenu;
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                                Console.WriteLine("Daxil etdiyiniz guide uygun worker tapilmadi!");
                                                goto RetryGuid;

                                            }
                                        }
                                    }
                                }
                            }
                            else goto EmployerMenu;
                            break;
                        }
                    case 1:
                        {
                            while (true)
                            {
                                choise = ConsoleHelper.MultipleChoice(5, 1, 1, true, "All my vacancies", "Add vacancy", "Update vacancy", "Delete vacancy", "Back to");
                                switch (choise)
                                {
                                    case 0:
                                        {
                                            Console.Clear();
                                            emp.ShowAllVacancy(db);
                                            Console.Write("Click any to go back...");
                                            Console.ReadLine();
                                            break;
                                        }
                                    case 1:
                                        {
                                            Console.Clear();
                                            emp.Vacancies.Add(GetIncludeVacancyInfo(db));
                                            FileProcessJson.JsonFileWriteAllText("Employers", db.Employers);
                                            Console.WriteLine("Vacancy is added...");
                                            Thread.Sleep(1500);
                                            Console.WriteLine("The vacancy was successfully added.");
                                            break;
                                        }
                                    case 2:
                                        {
                                            Console.Clear();
                                            if (emp.Vacancies == null)
                                            {
                                                Console.WriteLine("There is currently no vacancy!");
                                                Thread.Sleep(1500);
                                            }
                                            else
                                            {
                                                int i = 0;
                                                emp.Vacancies.ForEach(v =>
                                                {
                                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                                    Console.WriteLine($"{++i}) Vacancy guid: {v.Guid}");
                                                    Console.ResetColor();
                                                    v.Show(db);
                                                });
                                                Console.Write("Enter the vacancy guid you want to update: ");
                                                string guid = Console.ReadLine();
                                                if (emp.Vacancies.Exists(e => e.Guid.ToString() == guid))
                                                {
                                                    Vacancy tempVacancy = new();
                                                    tempVacancy = emp.Vacancies.Find(e => e.Guid.ToString() == guid);
                                                    Console.Clear();
                                                UpdateRetry:
                                                    int choiseUpdate = ConsoleHelper.MultipleChoice(5, 2, 1, true, "Category", "Work name", "Work city", "Edugation Degree", "Age", "Experience", "Languages", "Salary", "Job description", "Requirements", "Back to");
                                                    Console.Clear();
                                                    switch (choiseUpdate)
                                                    {
                                                        case 0:
                                                            {
                                                                Console.WriteLine($"Current category: {db.Categories.Find(c => c.Id == tempVacancy.CategoryId).Name}");
                                                                Console.WriteLine("Choise new category and subcategory: ");
                                                                String[] array = new String[db.Categories.ToList().Count];
                                                                for (int k = 0; k < db.Categories.ToList().Count; k++)
                                                                    array[k] = db.Categories.ToArray()[k].Name;
                                                                choise = ConsoleHelper.MultipleChoice(5, 3, 3, false, array);
                                                                uint filterCatId = db.Categories.ToList().Find(c => c.Name == array[choise]).Id;
                                                                tempVacancy.CategoryId = filterCatId;
                                                                array = new String[db.SubCategories.Count(s => s.CategoryId == filterCatId)];
                                                                for (int j = 0; j < db.SubCategories.Count(s => s.CategoryId == filterCatId); j++)
                                                                    array[j] = db.SubCategories.ToList().FindAll(s => s.CategoryId == filterCatId).ToArray()[j].Name;
                                                                Console.Clear();
                                                                Console.WriteLine($"Current subcategory: {db.SubCategories.Find(s => s.Id == tempVacancy.SubCategoryId).Name}");
                                                                choise = ConsoleHelper.MultipleChoice(5, 2, 1, false, array);
                                                                uint filterSubCatId = db.SubCategories.ToList().Find(s => s.Name == array[choise]).Id;
                                                                tempVacancy.SubCategoryId = filterSubCatId;
                                                                Console.Clear();
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 1:
                                                            {
                                                                Console.Clear();
                                                                Console.WriteLine($"Current work name: {tempVacancy.WorkName}");
                                                            RetryName:
                                                                Console.Write("Enter new work name: ");
                                                                tempVacancy.WorkName = Console.ReadLine();
                                                                if (!Helpers.IsValidCity(tempVacancy.WorkName)) goto RetryName;
                                                                else if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 2:
                                                            {
                                                                Console.Clear();
                                                                Console.WriteLine($"Current work city: {tempVacancy.WorkCity}");
                                                            RetryCity:
                                                                Console.Write("Enter new work city: ");
                                                                tempVacancy.WorkCity = Console.ReadLine();
                                                                if (!Helpers.IsValidCity(tempVacancy.WorkName)) goto RetryCity;
                                                                else if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 3:
                                                            {
                                                                Console.Clear();
                                                                Console.WriteLine($"Current education degree: {tempVacancy.EducationDegree}");
                                                                Console.WriteLine("Choise new education degree");
                                                                string[] educationChoicesArray = { "Science Degree", "Higher", "Incomplete Higher", "Secondary" };
                                                                int choice = ConsoleHelper.MultipleChoice(30, 2, 1, false, educationChoicesArray);
                                                                tempVacancy.EducationDegree = choice switch
                                                                {
                                                                    0 => educationChoicesArray[choice],
                                                                    1 => educationChoicesArray[choice],
                                                                    2 => educationChoicesArray[choice],
                                                                    3 => educationChoicesArray[choice],
                                                                    _ => "None"
                                                                };
                                                                Console.WriteLine();
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 4:
                                                            {
                                                                Console.Clear();
                                                                Console.WriteLine($"Current age: {tempVacancy.AgeMin}-{tempVacancy.AgeMax}");
                                                                Console.Write("Enter min age: ");
                                                                tempVacancy.AgeMin = uint.Parse(Console.ReadLine());
                                                                Console.Write("Enter max age: ");
                                                                tempVacancy.AgeMax = uint.Parse(Console.ReadLine());
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 5:
                                                            {
                                                                Console.Clear();
                                                                Console.WriteLine($"Current experience: {tempVacancy.Experience}");
                                                                Console.WriteLine("Choise new experience");
                                                                string[] experienceChoiseArray = { "No", "Less than 1 year", "From 1 to 3 years", "From 3 to 5 years", "More than 5 years" };
                                                                int choice = ConsoleHelper.MultipleChoice(24, 2, 1, false, experienceChoiseArray);
                                                                tempVacancy.Experience = choice switch
                                                                {
                                                                    0 => experienceChoiseArray[choice],
                                                                    1 => experienceChoiseArray[choice],
                                                                    2 => experienceChoiseArray[choice],
                                                                    3 => experienceChoiseArray[choice],
                                                                    4 => experienceChoiseArray[choice],
                                                                    _ => "None"
                                                                };
                                                                Console.WriteLine();
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 6:
                                                            {
                                                                Console.Clear();
                                                                Console.Write($"Current language(s): ");
                                                                tempVacancy.Languages.ForEach(v => Console.Write(v + " "));
                                                                List<string> languages = new();
                                                                Console.Write("\nEnter new language count: ");
                                                                int languageCount = int.Parse(Console.ReadLine());
                                                                if (languageCount == 0) tempVacancy.Languages = null;
                                                                string language;
                                                                for (int c = 0; c < languageCount; c++)
                                                                {
                                                                    Console.Write("Enter language: ");
                                                                    language = Console.ReadLine();
                                                                    languages.Add(language);
                                                                }
                                                                tempVacancy.Languages = languages;
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 7:
                                                            {
                                                                Console.Clear();
                                                                if (tempVacancy.Salary < tempVacancy.SalaryMax)
                                                                    Console.WriteLine($"Current salary: {tempVacancy.Salary}-{tempVacancy.SalaryMax}");
                                                                else if (tempVacancy.Salary > tempVacancy.SalaryMax)
                                                                    Console.WriteLine($"Current salary: {tempVacancy.SalaryMax}-{tempVacancy.Salary}");
                                                                else
                                                                    Console.WriteLine($"Current salary: {tempVacancy.Salary}");
                                                                Console.Write("Enter new salary min: ");
                                                                tempVacancy.Salary = double.Parse(Console.ReadLine());
                                                                Console.Write("Enter new salary max: ");
                                                                tempVacancy.SalaryMax = double.Parse(Console.ReadLine());
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 8:
                                                            {
                                                                Console.Clear();
                                                                Console.Write($"Current job description: {tempVacancy.JobDescription}");
                                                                Console.Write("Enter new job description: ");
                                                                tempVacancy.JobDescription = Console.ReadLine();
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;
                                                            }
                                                        case 9:
                                                            {
                                                                Console.Clear();
                                                                Console.Write($"Current requirements: {tempVacancy.Requirements}");
                                                                Console.Write("Enter new requirements: ");
                                                                tempVacancy.Requirements = Console.ReadLine();
                                                                if (IsReplyUpdateVacancy()) goto UpdateRetry;
                                                                break;

                                                            }
                                                        case 10: continue;
                                                    }
                                                    FileProcessJson.JsonFileWriteAllText("Employers.json", db.Employers);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("There is no vacancy to match the id you entered!");
                                                    Thread.Sleep(1300);
                                                }
                                            }
                                            break;
                                        }
                                    case 3:
                                        {
                                            Console.Clear();
                                            if (emp.Vacancies == null)
                                            {
                                                Console.WriteLine("There is no vacancy at the moment!");
                                                Thread.Sleep(1500);
                                            }
                                            else
                                            {
                                                int i = 0;
                                                emp.Vacancies.ForEach(cv =>
                                                {
                                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                                    Console.WriteLine($"{++i}) Vacancy guid: {cv.Guid}");
                                                    Console.ResetColor();
                                                    cv.Show(db);
                                                });
                                                Console.Write("Enter the guide of the vacancy you want to delete: ");
                                                string guid = Console.ReadLine();
                                                if (emp.Vacancies.Exists(g => g.Guid.ToString() == guid))
                                                {
                                                    Console.WriteLine("The vacancy you selected will be deleted...");
                                                    Thread.Sleep(1300);
                                                    Console.WriteLine("The vacancy was successfully deleted.");
                                                    emp.Vacancies.Remove(emp.Vacancies.Find(g => g.Guid.ToString() == guid));
                                                    FileProcessJson.JsonFileWriteAllText<List<Employer>>("Employers.json", db.Employers);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("There is no vacancy for the id you entered!");
                                                    Thread.Sleep(1300);
                                                }
                                            }
                                            break;
                                        }
                                    case 4:
                                        goto EmployerMenu;
                                }
                            }
                        }
                    case 2:
                        {
                            Console.Clear();
                            if (emp.Notifications == null)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine("There are currently no active notifications!");
                                Console.ResetColor();
                                Thread.Sleep(1500);
                            }
                            else
                            {
                            RetryNotf:
                                Console.Clear();
                                emp.ShowAllNotf();
                                Console.Write("Enter the id you want to view: ");
                                int id = int.Parse(Console.ReadLine());
                                if (emp.Notifications.Exists(n => n.NotificationId == id))
                                {
                                    Console.Clear();
                                    Console.WriteLine("\n-------------------------------------");
                                    string guid = emp.Notifications.Find(n => n.NotificationId == id).PostGuid;
                                    Worker worker = new();
                                    worker = db.Workers.Find(w => w.Cvs.Exists(c => c.Guid.ToString() == guid));
                                    worker.Notifications = new();
                                    worker.Show();
                                    Console.WriteLine("--------- Cv info ---------");
                                    emp.IncomingCVs.Find(i => i.Guid.ToString() == guid).Show(db);
                                    choise = ConsoleHelper.MultipleChoice(1, 0, 2, false, "To hire", "Reject");
                                    switch (choise)
                                    {
                                        case 0:
                                            {
                                                Notification notf = new() { Title = "New notf by employer!", Text = "Congratulations, you have been accepted!", FromUser = emp.Name + " " + emp.Surname };
                                                worker.Cvs.Remove(worker.Cvs.Find(c => c.Guid.ToString() == guid));
                                                Console.Clear();
                                                Console.WriteLine($"The notification is sent to {worker.Name}...");
                                                worker.Notifications.Add(notf);
                                                Network.SendNotf(emp, worker, notf);
                                                break;
                                            }
                                        case 1:
                                            {
                                                Notification notf = new() { Title = "New notf by employer!", Text = "Your terms do not match. Good luck for you!", FromUser = emp.Name + " " + emp.Surname };
                                                Console.Clear();
                                                Console.WriteLine($"The notification is sent to {worker.Name}...");
                                                worker.Notifications.Add(notf);
                                                Network.SendNotf(emp, worker, notf);
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    emp.DeleteNotfById(id);
                                }
                                else
                                {
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine("No matching notification was found for the id you entered!");
                                    Console.ResetColor();
                                    Thread.Sleep(1400);
                                    goto RetryNotf;
                                }

                            }
                            goto EmployerMenu;
                        }
                    case 3:
                        ProgramManagment.Start();
                        break;
                }

            }
        }
        private static Vacancy GetIncludeVacancyInfo(Database db)
        {
            List<string> languages = new();
            Vacancy vacancy = new();
            String[] array = new String[db.Categories.ToList().Count];
            for (int i = 0; i < db.Categories.ToList().Count; i++)
                array[i] = db.Categories.ToArray()[i].Name;
            int choise = ConsoleHelper.MultipleChoice(5, 1, 3, true, array);
            uint filterCatId = db.Categories.ToList().Find(c => c.Name == array[choise]).Id;
            vacancy.CategoryId = filterCatId;
            array = new String[db.SubCategories.Count(s => s.CategoryId == filterCatId)];
            for (int i = 0; i < db.SubCategories.Count(s => s.CategoryId == filterCatId); i++)
            {
                array[i] = db.SubCategories.ToList().FindAll(s => s.CategoryId == filterCatId).ToArray()[i].Name;
            }
            choise = ConsoleHelper.MultipleChoice(5, 1, 1, true, array);
            uint filterSubCatId = db.SubCategories.ToList().Find(s => s.Name == array[choise]).Id;
            vacancy.SubCategoryId = filterSubCatId;
            Console.Clear();
        RetryName:
            Console.Write("Enter work name: ");
            vacancy.WorkName = Console.ReadLine();
            if (!Helpers.IsValidName(vacancy.WorkName)) goto RetryName;
            Console.Clear();
        RetryCity:
            Console.Write("Enter work city: ");
            vacancy.WorkCity = Console.ReadLine();
            if (!Helpers.IsValidCity(vacancy.WorkCity)) goto RetryCity;
            Console.Clear();
            Console.WriteLine("Choise education degree");
            string[] educationChoicesArray = { "Science Degree", "Higher", "Incomplete Higher", "Secondary" };
            int choice = ConsoleHelper.MultipleChoice(25, 1, 1, false, educationChoicesArray);
            vacancy.EducationDegree = choice switch
            {
                0 => educationChoicesArray[choice],
                1 => educationChoicesArray[choice],
                2 => educationChoicesArray[choice],
                3 => educationChoicesArray[choice],
                _ => "None"
            };
            Console.Clear();
            Console.WriteLine("Choise experience");
            string[] experienceChoiseArray = { "Less than 1 year", "From 1 to 3 years", "From 3 to 5 years", "More than 5 years" };
            choice = ConsoleHelper.MultipleChoice(26, 1, 1, false, experienceChoiseArray);
            vacancy.Experience = choice switch
            {
                0 => experienceChoiseArray[choice],
                1 => experienceChoiseArray[choice],
                2 => experienceChoiseArray[choice],
                3 => experienceChoiseArray[choice],
                _ => "None"
            };
            Console.Clear();
            Console.Write("Enter count of languages required: ");
            int languageCount = int.Parse(Console.ReadLine());
            if (languageCount == 0) vacancy.Languages = null;
            string language;
            for (int i = 0; i < languageCount; i++)
            {
                Console.Write("Enter language: ");
                language = Console.ReadLine();
                languages.Add(language);
            }
            vacancy.Languages = languages;
            Console.Clear();
            Console.Write("Enter min age: ");
            vacancy.AgeMin = uint.Parse(Console.ReadLine());
            Console.Write("Enter max age: ");
            vacancy.AgeMax = uint.Parse(Console.ReadLine());
            Console.Clear();
            Console.Write("Enter work requirements: ");
            vacancy.Requirements = Console.ReadLine();
            Console.Clear();
            Console.Write("Enter work description: ");
            vacancy.JobDescription = Console.ReadLine();
            Console.Clear();
            Console.Write("Enter salary min: ");
            vacancy.Salary = double.Parse(Console.ReadLine());
            Console.Write("Enter salary max: ");
            vacancy.SalaryMax = double.Parse(Console.ReadLine());
            return vacancy;
        }
        private static bool IsReplyUpdateVacancy()
        {
        RetryChoise:
            Console.Write("You want to update again?(Enter or Esc): ");
            var retry = Console.ReadKey();
            if (retry.Key == ConsoleKey.Enter)
            {
                return true;
            }
            else if (retry.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("\nTThe vacancy of your choice is update...");
                Thread.Sleep(1300);
                Console.WriteLine("The vacancy was successfully updated");
                Thread.Sleep(700);
                return false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Check again for incorrect selection!");
                Console.ResetColor();
                Thread.Sleep(800);
                goto RetryChoise;
            }
        }
    }
}
