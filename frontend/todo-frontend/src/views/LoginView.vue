<script setup lang="ts">
import api from '@/api/api';
import type { LoginRequest } from '@/models/auth';
import { ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter()

const email = ref<string>('')
const password = ref<string>('')

async function login() {
    try{
        const request : LoginRequest = {
            email:email.value,
            password:password.value
        }
        const response = await api.post('/Auth/login',request)
        localStorage.setItem('token', response.data.token)
        router.push('/todos')
    }
    catch(error){
        console.log(error)
        alert('Login failed.')
    }
}

</script>

<template>
  <h1>Login</h1>
  <label>Email: </label>
  <input type="email" v-model="email" placeholder="Write your email address..." required />
  <br><br>
  <label>Password: </label>
  <input type="password" v-model="password" placeholder="Write your password..." required />
  <br><br>
  <button @click="login">Login</button>
  <br><br>
  <RouterLink to="/register">Register</RouterLink>
</template>

<style scoped>

</style>
