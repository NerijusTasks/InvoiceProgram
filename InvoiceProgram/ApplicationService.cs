using InvoiceProgram.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace InvoiceProgram
{
    public class ApplicationService
    {
        private ApplicationState _applicationState = ApplicationState.ReadAmount;
        private bool _isPayingVat;
        private decimal _totalAmount;
        private CountryModel? _supplierCountry;
        private CountryModel? _clientCountry;
        private decimal _vatPercentage;

        public void StartProgram()
        {
            while (true)
            {
                switch (_applicationState)
                {
                    case ApplicationState.ReadAmount:

                        _totalAmount = ReadNumericValue(
                            "Enter the amount to be paid by the customer"
                        );

                        _applicationState = ApplicationState.CheckIsPayingVat;

                        break;

                    case ApplicationState.CheckIsPayingVat:

                        _isPayingVat = GetAnswer("Is the seller a VAT payer? Y/N ");

                        if (!_isPayingVat)
                        {
                            _applicationState = ApplicationState.CalculateTotalInvoice;
                        }
                        else
                        {
                            _applicationState = ApplicationState.ReadSupplierCountry;
                        }
                        break;

                    case ApplicationState.ReadSupplierCountry:
                        _supplierCountry = GetCountryNameAndRegion(
                            "Enter the seller's country name"
                        );
                        _applicationState = ApplicationState.ReadClientCountry;
                        break;

                    case ApplicationState.ReadClientCountry:
                        _clientCountry = GetCountryNameAndRegion("Enter the payer's country name");
                        _applicationState = ApplicationState.CheckIsVatNeeded;
                        break;

                    case ApplicationState.CheckIsVatNeeded:
                        if (
                            _clientCountry.Country == _supplierCountry.Country || _clientCountry.Region == "Europe"
                        )
                        {
                            _applicationState = ApplicationState.ReadVatValue;
                            _vatPercentage = ReadNumericValue("Enter the VAT of the buyer's country");
                            _applicationState = ApplicationState.CalculateTotalInvoice;
                            break;

                            if (GetAnswer("Do you want to create a new invoice? Y/N "))
                            {
                                _applicationState = ApplicationState.ReadAmount;
                                break;
                            }
                            else
                            {
                                return;
                            }
                        }
                        break;

                    case ApplicationState.ReadVatValue:
                        _vatPercentage = ReadNumericValue("Enter the VAT of the buyer's country");
                        _applicationState = ApplicationState.CalculateTotalInvoice;
                        break;

                    case ApplicationState.CalculateTotalInvoice:
                        CalculateTotalInvoice();
                        if (GetAnswer("Do you want to create a new invoice? Y/N "))
                        {
                            _applicationState = ApplicationState.ReadAmount;
                            break;
                        }
                        else
                        {
                            return;
                        }

                    default:
                        Console.WriteLine("Fatal error :`(");
                        break;
                }
            }
        }

        public bool GetAnswer(string message)
        {
            while (true)
            {
                Console.WriteLine(message);

                string answer = Console.ReadLine().ToUpper();

                if (answer == "Y")
                {
                    return true;
                }

                if (answer == "N")
                {
                    return false;
                }
            }
        }

        public CountryModel? GetCountryData(string countryName)
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("Loading...");
                string url = $"https://api.first.org/data/v1/countries?q={countryName}";
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    Dictionary<string, CountryModel> countries =
                        new Dictionary<string, CountryModel>();
                    var json = response.Content.ReadAsStringAsync().Result;
                    JToken data = JsonConvert.DeserializeObject<ResponseModel>(json).Data;

                    if (data.Type == JTokenType.Object)
                    {
                        countries = data.ToObject<Dictionary<string, CountryModel>>();
                    }
                    else if (data.Type == JTokenType.Array)
                    {
                        Console.WriteLine("Couldn't find anything, try again");
                        return null;
                    }

                    var countriesList = countries.Select(x => x.Value).ToList();

                    if (countriesList.Count == 1 && countriesList[0].Country == countryName)
                    {
                        return countriesList[0];
                    }
                    else
                    {
                        Console.WriteLine(
                            "found more than one or incompletely entered country name"
                        );
                        return null;
                    }
                }
                else
                {
                    throw new Exception("Failed to fetch country data from API.");
                }
            }
        }

        public decimal ReadNumericValue(string message)
        {
            while (true)
            {
                Console.WriteLine(message);
                string input = Console.ReadLine();
                decimal insertedSum;

                if (decimal.TryParse(input, out insertedSum))
                {
                    if (insertedSum <= 0)
                    {
                        Console.WriteLine("Bad input");
                    }
                    else
                    {
                        return insertedSum;
                    }
                }
                else
                {
                    Console.WriteLine("Only numbers");
                }
            }
        }

        public void CalculateTotalInvoice()
        {
            if (!_isPayingVat)
            {
                Console.WriteLine($"Sum: {_totalAmount}");
                Console.WriteLine($"VAT: 0 %");
                Console.WriteLine($"Total sum: {_totalAmount}");
            }
            else
            {
                decimal result = _totalAmount + (_totalAmount * (_vatPercentage / 100));
                Console.WriteLine($"Sum: {_totalAmount}");
                Console.WriteLine($"VAT: {_vatPercentage}%");
                Console.WriteLine($"Total sum: {result}");
            }
        }

        public CountryModel GetCountryNameAndRegion(string message)
        {
            while (true)
            {
                Console.WriteLine(message);

                string? answer = Console.ReadLine();

                if (answer != null && Regex.IsMatch(answer, @"^[a-zA-Z]+$"))
                {
                    var receivedCountryData = GetCountryData(answer);

                    if (receivedCountryData != null)
                    {
                        return receivedCountryData;
                    }
                }
            }
        }
    }
}
