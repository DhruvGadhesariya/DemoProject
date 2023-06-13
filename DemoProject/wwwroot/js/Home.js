
function Search(pg, finder, sort) {

    var pagesize = $('#selectentities').find(":selected").val();
    var obj = GetFilter();

    if (pg == undefined) {
        pg = 1;
    }

    console.log(pagesize);
    $.ajax({
        url: "/Home/Search",
        type: "post",
        data: {
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function (data) {
            $('#dataUsers').html(data);
        }
    })
    $.ajax({
        url: "/Home/Pagination",
        type: "post",
        data: {
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function (data) {
            $('#userpagination').html(data);
        }
    })
}

function downloadData(format, pageSize, currentPage) {

    var selectedFormat = format;
    var obj = GetFilter();

    $.ajax({
        url: '/Home/DownloadData',
        type: 'POST',
        data: {
            format: selectedFormat,
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            PageSize: pageSize,
            Pg: currentPage
        },
        success: function () {
            console.log('Download success');
        },
        error: function () {
            console.log('Download failed');
        }
    });
}

function GetFilter() {
    var SearchFname = $("input[name='SearchFname']").val();
    var SearchLname = $("input[name='SearchLname']").val();
    var SearchEmail = $("input[name='SearchEmail']").val();

    var searches = [];

    if (SearchFname.length > 0) {
        searches.SearchFname = SearchFname;
    }

    if (SearchLname.length > 0) {
        searches.SearchLname = SearchLname;
    }

    if (SearchEmail.length > 0) {
        searches.SearchEmail = SearchEmail;
    }

    return searches;
}

function GetCity() {

    var countryId = $('#ocountries').find(":selected").val();
    var ProductId = $('#products').find(":selected").val();
    $.ajax({

        url: "/Home/GetCity",
        method: "POST",

        data: {
            "countryId": countryId,
            "ProductId": ProductId
        },

        success: function (data) {

            data = JSON.parse(data);
            $("#CityForOrder").empty();

            data.forEach((name) => {

                document.getElementById("CityForOrder").innerHTML += `
                                                           <option value="${name.CityId}" name="CityId">
                                                                   ${name.Name}
                                                           </option>`;
            })


        },
        error: function (request, error) {
            console.log(error);
        }

    })
}

function addUser() {

    var fname = $('#ufname').val().trim();
    var lname = $('#ulname').val().trim();
    var email = $('#uemail').val().trim();
    var password = $('#upassword').val().trim();
    var country = $('#ucountries').find(":selected").val();
    var city = $('#selectCityList').find(":selected").val();

    if (!(fname) || !(lname) || !(email) || !(password) || !(country) || !(city)) {
        toastr.error("Please Enter All The Data!!")
    } else {
        $.ajax({
            url: '/Home/AddUserByMe',
            type: 'POST',
            datatype: 'html',
            data: {
                Fname: fname,
                Lname: lname,
                Email: email,
                Password: password,
                CountryId: country,
                CityId: city
            },
            success: function () {
                location.reload()
            }

        });
    }


}

function clearModal() {

    $("#userAdd").trigger("reset");
    $("#selectCityList").empty();
    $("#addOrder").trigger("reset");
    $("#empAdd").trigger("reset");
    $('#timestatus').html("");
    $("#CityForOrder").empty();
}

function GetUserData(userId) {

    $.ajax({
        url: '/Home/GetDataOfUser',
        type: 'POST',
        datatype: 'json',
        data: {
            userId: userId
        },
        success: function (data) {

            var json = JSON.parse(data);

            console.log(json);

            if (json[0].FirstName != null) {
                document.getElementById('fname').value = json[0].FirstName;
            } else {
                document.getElementById('fname').value = "";
            }

            if (json[0].LastName != null) {
                document.getElementById('lname').value = json[0].LastName;
            } else {
                document.getElementById('lname').value = "";
            }

            if (json[0].Email != null) {
                document.getElementById('email').value = json[0].Email;
            } else {
                document.getElementById('email').value = "";
            }

            $("#selectCityLists").empty();
            document.getElementById('selectCityLists').innerHTML += `<option selected value="${json[0].CityId}">${json[0].CityName} </option>
                `;
            document.getElementById('countries').value = json[0].CountryId;
            document.getElementById('userid').value = json[0].UserId;
        }

    })
}

function AfterEditAddUser() {

    var fname = $('#fname').val().trim();
    var lname = $('#lname').val().trim();
    var email = $('#email').val().trim();
    var countryId = $('#countries').find(":selected").val();
    var cityId = $('#selectCityLists').find(":selected").val();
    var userId = $('#userid').val().trim();

    if (!(fname) || !(lname) || !(email) || !(countryId) || !(cityId)) {
        toastr.error("Please Enter All The Data!!");
    }
    else {
        $.ajax({
            url: '/Home/EditUserByMe',
            type: 'POST',
            datatype: 'html',
            data: {
                Fname: fname,
                Lname: lname,
                Email: email,
                CountryId: countryId,
                CityId: cityId,
                UserId: userId,
            },
            success: function () {
                location.reload()
            }

        });
    }

}

function removeByAdmin(Id) {


    if (confirm("Do you want to remove This ?")) {
        console.log(Id);

        $.ajax({

            url: '/Home/RemoveByAdmin',
            type: 'POST',
            datatype: 'html',
            data: {
                Id: Id,
            },
            success: function () {
                location.reload();
            }

        });
    }
    else {
        toastr.error("You have clicked on cancel!!")
    }


}

function GetCityForUser() {

    var countryId = $('#ucountries').find(":selected").val();

    $.ajax({

        url: "/Home/GetCity",
        method: "POST",

        data: {
            "countryId": countryId
        },

        success: function (data) {

            data = JSON.parse(data);
            $("#selectCityList").empty();

            data.forEach((name) => {

                document.getElementById("selectCityList").innerHTML += `
                                                                           <option value="${name.CityId}">
                                                                                   ${name.Name}
                                                                           </option>`;
            })


        },
        error: function (request, error) {
            console.log(error);
        }


    })
}

function AddProducts() {

    var ProductId = $('#products').find(":selected").val();
    var country = $('#ocountries').find(":selected").val();
    var city = $('#CityForOrder').find(":selected").val();
    var from = $('#from').val();
    var to = $('#to').val();

    $.ajax({
        url: '/Home/OrderProduct',
        type: 'GET',
        datatype: 'html',
        data: {
            ProductId: ProductId,
            CountryId: country,
            CityId: city,
            From: from,
            To: to
        },
        success: function (response) {
            if (response == "falseTime") {
                $('#timestatus').html("Order for this time is already placed!!");
                $('#timestatus').css('color', 'red');
            }
            else if (response == "notAvailable") {
                $('#timestatus').html("Product is not Available for your Country!!");
                $('#timestatus').css('color', 'red');
            }
            else {
                location.reload();
                $('#addProducts').modal('hide');
                clearModal();
            }
        }
    });

}

function GetCheckedCountryIds(id) {

    $.ajax({

        url: "/Products/GetCity",
        method: "POST",

        data: {
            "countryId": id
        },

        success: function (data) {

            data = JSON.parse(data);
            $("#cityListForOrder_" + id).empty();

            data.forEach((name) => {

                document.getElementById("cityListForOrder_" + id).innerHTML += `
                                                     <div class=" p-0 mx-2 d-flex">
                                                       <input type="checkbox" class="form-check-input city_${name.CityId}" name="city"  value="${name.CountryId}" id="${name.CityId}"/>
                                                       <label class="form-check-label ms-5" for="${name.CityId}">
                                                          ${name.Name}
                                                       </label>
                                                     </div>
                                                  `;
            });
        },
        error: function (request, error) {
            console.log(error);
        }


    });
}

function AddProductByAdmin() {
    var productName = $('#productName').val().trim();
    var productShared = $('#productShared').find(":selected").val();
    var cityMappings = {};

    var cityCheckboxes = document.querySelectorAll('input[type="checkbox"][name="city"]:checked');

    cityCheckboxes.forEach(function (checkbox) {
        var countryId = checkbox.value;
        var cityId = checkbox.id;

        if (!cityMappings[countryId]) {
            cityMappings[countryId] = [];
        }
        cityMappings[countryId].push(cityId);
    });

    $.ajax({
        url: '/Products/AddProductByAdmin',
        type: 'POST',
        datatype: 'json',
        data: {
            productName: productName,
            productShared: productShared,
            CityMappings: JSON.stringify(cityMappings)
        },
        success: function (response) {
            if (response.success) {
                alert(response.message);
                location.reload();
            } else {
                alert(response.message);
            }
        }
    });
}

function EditProductByAdmin(productId) {
    var productName = $('#productName').val().trim();
    var productShared = $('#productShared').find(":selected").val();
    var cityMappings = {};

    var cityCheckboxes = document.querySelectorAll('input[type="checkbox"][name="editcity"]:checked');

    cityCheckboxes.forEach(function (checkbox) {
        var countryId = checkbox.value;
        var cityId = checkbox.id;

        if (!cityMappings[countryId]) {
            cityMappings[countryId] = [];
        }
        cityMappings[countryId].push(cityId);
    });

    console.log(cityMappings);

    $.ajax({
        url: '/Products/EditProductByAdmin',
        type: 'POST',
        datatype: 'json',
        data: {
            productId: productId,
            productName: productName,
            productShared: productShared,
            CityMappings: JSON.stringify(cityMappings)
        },
        success: function (response) {
            if (response.success) {
                alert(response.message);
                window.location.href = '/Products/Products';
            } else {
                alert(response.message);
            }
        }
    });
}

function RemoveProductByAdmin(Id) {


    if (confirm("Do you want to remove This ?")) {
        console.log(Id);

        $.ajax({

            url: '/Products/RemoveByAdmin',
            type: 'POST',
            datatype: 'html',
            data: {
                Id: Id,
            },
            success: function () {
                location.reload();
            }
        });
    }
    else {
        toastr.error("You have clicked on cancel!!");
    }
}

function SearchProducts(pg, finder, sort) {

    var pagesize = $('#selectproducts').find(":selected").val();
    var Name = $("input[name='Name']").val();

    if (pg == undefined) {
        pg = 1;
    }

    $.ajax({
        url: "/Products/SearchProducts",
        type: "post",
        data: {
            Name: Name,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function () {

        }
    })
    $.ajax({
        url: "/Products/Pagination",
        type: "post",
        data: {
            Name: Name,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function () {

        }
    })
}

function SendMail() {
    $.ajax({
        url: "/Home/SendEmailForOrderAsync",
        type: "post",
        success: function () {

        }
    })
}

function SearchEmp(pg, finder, sort) {

    var pagesize = $('#selectentities').find(":selected").val();
    var obj = GetFilter();

    if (pg == undefined) {
        pg = 1;
    }

    $.ajax({
        url: "/Employee/Search",
        type: "post",
        data: {
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function (data) {
            $('#dataEmp').html(data);
        }
    })
    $.ajax({
        url: "/Employee/Pagination",
        type: "post",
        data: {
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function (data) {
            $('#emppagination').html(data);
        }
    })
}


