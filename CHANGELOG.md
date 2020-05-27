# 2.1.0
- Breaking: Added IEvaluator.EvaluatorInfo property instead Name
- Ordering evaluators
- If evaluator skips value (returns the same value) then next evaluators take chance

# 2.0.0
- Breaking: Added key to IEvaluator to support evaluators with context knowlege
- Fixed parse recursion whem close bracket not found
- Change: configurationValue evaluator does not replaces dots

# 1.8.0
- ${include} only if path is not null or empty after resolve

# 1.7.0
- Microsoft.Extensions updated to 3.1.3

# 1.6.0
- IsSecretConfigurationKey added to StartupConfiguration
- ILogger and ILoggerFactory added to IBuildContext
- Evaluators can be set to ConfigurationBuilder.Properties for usin in include blocks

# 1.5.0
- Limited support of placeholders for ${include}

# 1.4.0
- ${include} can be an array of pathes or single path
- Some unused code removed and simplified

# 1.3.0
- Recursive evaluations

# 1.2.0
- Property evaluation fixed
- IPropertyEvaluator interface changed

# 1.1.0 - 15.05.2020
- All merged to one package MicroElements
- Microsoft.Extensions updated to 3.1.0

# 1.0.0 - 01.10.2017
- Development started
