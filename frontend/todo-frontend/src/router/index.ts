import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import TodosView from '@/views/TodosView.vue'
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/register'
    },
    {
      path: '/register',
      component: RegisterView
    },
    {
      path: '/login',
      component: LoginView
    },
    {
      path: '/todos',
      component: TodosView
    },
  ],
})

router.beforeEach((to) => {
  if (to.path === '/todos' && !localStorage.getItem('token')) {
    return '/login'
  }
})

export default router
