import { createRouter, createWebHistory } from 'vue-router'
import authService from '@/services/authService'
import { routerLogger } from '@/utils/logger';

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/DashboardView.vue'),
    meta: { requiresAuth: true, roles: ['admin', 'manager', 'developer'] }
  },

  // Admin routes
  {
    path: '/admin/companies',
    name: 'AdminCompanies',
    component: () => import('@/views/admin/CompaniesView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  {
    path: '/admin/projects',
    name: 'AdminProjects',
    component: () => import('@/views/admin/ProjectsView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  {
    path: '/admin/statuses',
    name: 'AdminStatuses',
    component: () => import('@/views/admin/TaskStatusesView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  {
    path: '/admin/task-types',
    name: 'AdminTaskTypes',
    component: () => import('@/views/admin/TaskTypesView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  {
    path: '/sprints',
    name: 'Sprints',
    component: () => import('@/views/SprintsView.vue'),
    meta: { requiresAuth: true, roles: ['admin', 'manager'] }
  },
  {
    path: '/admin/tasks',
    name: 'AdminTasks',
    component: () => import('@/views/admin/AdminTasksView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  {
    path: '/admin/users',
    name: 'AdminUsers',
    component: () => import('@/views/admin/UsersView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  {
    path: '/admin/teams',
    name: 'AdminTeams',
    component: () => import('@/views/admin/AdminTeamsView.vue'),
    meta: { requiresAuth: true, roles: ['admin'] }
  },
  // Manager routes
  {
    path: '/manager/team',
    name: 'ManagerTeam',
    component: () => import('@/views/manager/ManagerTeamView.vue'),
    meta: { requiresAuth: true, roles: ['manager'] }
  },
  {
    path: '/manager/tasks',
    name: 'ManagerDevTasks',
    component: () => import('@/views/manager/ManagerTasksView.vue'),
    meta: { requiresAuth: true, roles: ['admin', 'manager'] }
  },
  // Developer routes
  {
    path: '/developer/tasks',
    name: 'DeveloperTasks',
    component: () => import('@/views/developer/TasksView.vue'),
    meta: { requiresAuth: true, roles: ['admin', 'manager', 'developer'] }
  },
  // Common routes
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/ProfileView.vue'),
    meta: { requiresAuth: true, roles: ['admin', 'manager', 'developer'] }
  },

  // Error routes
  {
    path: '/unauthorized',
    name: 'Unauthorized',
    component: () => import('@/views/UnauthorizedView.vue'),
    meta: { requiresAuth: true }
  },

  // 404 route
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/NotFoundView.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const isAuthenticated = authService.isAuthenticated();
  const user = authService.getStoredUser();

  routerLogger.log('Navigation:', { to: to.path, from: from.path, isAuthenticated, user });

  if (to.meta.requiresAuth) {
    if (!isAuthenticated) {
      routerLogger.log('No auth, redirecting to /login');
      next('/login');
      return;
    }

    if (to.meta.roles && user && user.role) {
      const hasRequiredRole = (to.meta.roles as string[]).includes(user.role);
      if (!hasRequiredRole) {
        routerLogger.log('No role access, redirecting to /unauthorized');

        if (to.path !== '/unauthorized') {
          next({
            path: '/unauthorized',
            query: {
              roles: JSON.stringify(to.meta.roles),
              attempted: to.path
            }
          });
          return;
        }
      }
    } else if (!user || !user.role) {
      routerLogger.warn('No user data, waiting for update');
    }
  }

  if (to.path === '/login' && isAuthenticated) {
    routerLogger.log('Logged in, redirecting from /login to /dashboard');
    next('/dashboard');
    return;
  }

  next();
});

export default router