;(function ($) {


  var forms = $('.wpcf7-form'); 
  forms.each(function(){
      var formId = $(this).find('input[name="_wpcf7"]').val();   
      $('.uacf7-form-'+formId).find('input.uacf7_country_dropdown_with_flag').each(function(){
        
        var fieldId = jQuery(this).attr('id');
        var defaultCountry = jQuery(this).attr('country-code');
        var onlyCountries = jQuery(this).attr('only-countries');  
        if(typeof onlyCountries !== "undefined" && onlyCountries != ''){
          onlyCountries = JSON.parse(onlyCountries);
        }else{
          onlyCountries = '';
        } 
        
        $("#"+fieldId).countrySelect({ 
          defaultCountry: defaultCountry,
          onlyCountries: onlyCountries,
          responsiveDropdown: true,
          preferredCountries: []
        });  
      });      
}); 


})(jQuery);

