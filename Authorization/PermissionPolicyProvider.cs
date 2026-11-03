// BackendGeneral.Api/Authorization/PermissionPolicyProvider.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BackendGeneral.Api.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    /// <summary>
    /// ⚠️ MODO DESARROLLO: si true, cualquier policy (ej. "catalogo.ver")
    /// se resuelve como "usuario autenticado, sin validar permiso".
    /// Cambiar a false antes de pasar a producción.
    /// </summary>
    private const bool DEV_BYPASS_PERMISSIONS = true;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // ─────────────────────────────────────────────────────────────
        // TODO(seguridad): Cuando DEV_BYPASS_PERMISSIONS = false,
        //   se valida contra el claim "permission" del JWT.
        //   Mientras esté en true, cualquier policy existente concede acceso
        //   al usuario autenticado (solo requiere Bearer token válido).
        //
        //   ⚠️ NO dejar en true en producción: expone todos los endpoints
        //      a cualquier usuario con sesión válida.
        //
        //   Formato esperado en los controllers:
        //       [Authorize(Policy = "catalogo.ver")]
        //       [Authorize(Policy = "pedidos.crear")]
        //       [Authorize(Policy = "caja.operar")]
        //   O con tu atributo custom:
        //       [HasPermission("catalogo.ver")]
        // ─────────────────────────────────────────────────────────────

        if (DEV_BYPASS_PERMISSIONS)
        {
            // Comodín: cualquier policy => solo requiere autenticado.
            // Así "catalogo.ver", "pedidos.crear", "caja.operar", etc. resuelven 200.
            var permissive = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(permissive);
        }

        // ── Modo real (cuando reactives seguridad) ──
        if (policyName.StartsWith(HasPermissionAttribute.PolicyPrefix,
                                  StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(HasPermissionAttribute.PolicyPrefix.Length);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Si no es una permission policy, delega al provider por defecto
        return _fallback.GetPolicyAsync(policyName);
    }
}
/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BackendGeneral.Api.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // ¿Empieza con "PERM:"?
        if (policyName.StartsWith(HasPermissionAttribute.PolicyPrefix,
                                  StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(HasPermissionAttribute.PolicyPrefix.Length);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Si no es una permission policy, delega al provider por defecto
        return _fallback.GetPolicyAsync(policyName);
    }
}







*/
