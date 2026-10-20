<script setup lang="ts">
import api from '@/api/api';
import type { RegisterRequest } from '@/models/auth';
import { ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter()

const fullName = ref<string>('')
const email = ref<string>('')
const password = ref<string>('')

async function register() {
    try{
        const request : RegisterRequest = {
            fullName:fullName.value,
            email:email.value,
            password:password.value
        }
        const response = await api.post('/Auth/register',request)
        localStorage.setItem('token', response.data.token)
        router.push('/todos')
    }
    catch(error){
        console.log(error)
        alert('Register failed.')
    }
}

</script>

<template>
  <h1>Register</h1>
  <label>FullName: </label>
  <input type="text" v-model="fullName" placeholder="Write your fullname..." required />
  <br><br>
  <label>Email: </label>
  <input type="email" v-model="email" placeholder="Write your email address..." required />
  <br><br>
  <label>Password: </label>
  <input type="password" v-model="password" placeholder="Write your password..." required />
  <br><br>
  <button @click="register">Register</button>
  <br><br>
  <RouterLink to="/login">Login</RouterLink>
</template>

<style scoped>

</style>
