
# Problem Details in .Net 7 

The "Problem Detail" method is a machine-readable approach for displaying error information in an HTTP response, which is recommended to prevent the need for creating new error response formats for HTTP APIs. This approach is defined in the RFC7807 standard. However, in older versions of .NET, there is no standardized error payload that is generated when an unhandled exception occurs. This issue has been addressed in .NET 7 with the introduction of the IProblemDetailsService.