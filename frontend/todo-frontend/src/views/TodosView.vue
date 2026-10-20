<script setup lang="ts">
import api from '@/api/api';
import type { CreateTodoRequest, Todo, UpdateTodoRequest } from '@/models/todo';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter()

const todos = ref<Todo[]>([])

const id = ref<number>(0)
const title = ref<string>('')
const description = ref<string>('')
const isActive = ref<boolean>(true)
const createDate = ref<Date>(new Date)

async function loadTodos() {
    try{
        const response = await api.get('/Todos')
        todos.value = response.data
    }
    catch(error){
        console.log(error)
        alert('Load todos operation failed.')
    }
}

async function deleteTodo(id:number) {
    try{
        await api.delete(`/Todos/${id}`)
        loadTodos()
    }
    catch(error){
        console.log(error)
        alert('Delete todo operation failed.')
    }
}

async function saveTodo() {
    try{
        if(id.value === 0 ){
            const request : CreateTodoRequest = {
                title: title.value,
                description:description.value,
                isActive:isActive.value,
                createDate:new Date()
            }
            await api.post('/Todos',request)
        }
        else{
            const request : UpdateTodoRequest = {
                title: title.value,
                description:description.value,
                isActive:isActive.value,
                createDate:createDate.value,
                updateDate:new Date()
            }
            await api.put(`/Todos/${id.value}`,request)
        }
        clearForm()
        loadTodos()
    }
    catch(error){
        console.log(error)
        alert('Save todo operation failed.')
    }
}

function clearForm(){
    id.value = 0
    title.value = ''
    description.value = ''
    isActive.value = true
}

function editTodo(todo:Todo){
    id.value = todo.id
    title.value = todo.title
    description.value = todo.description
    isActive.value = todo.isActive
    createDate.value = todo.createDate
}

onMounted(() =>{
    loadTodos()
})

function logout(){
    localStorage.removeItem('token')
    router.push('/login')
}

</script>

<template>
<h1>Todo Management</h1>
<button @click="logout">Logout</button>
<hr>
<label>Title: </label>
<input type="text" v-model="title" placeholder="Write title..." required />
<br><br>
<label>Description: </label>
<input type="text" v-model="description" placeholder="Write description..." required />
<br><br>
<label>
    <input type="checkbox" v-model="isActive" />
    IsActive
</label>
<button @click="saveTodo">Save</button>
<button @click="clearForm" style="margin-left: 5px;">Clear</button>
<hr>
<table border="1">
    <thead>
        <tr>
            <th>Id</th>
            <th>Title</th>
            <th>Description</th>
            <th>IsActive</th>
            <th>CreateDate</th>
            <td>Actions</td>
        </tr>
    </thead>
    <tbody>
        <tr v-for="todo in todos" :key="todo.id">
            <td>{{ todo.id }}</td>
            <td>{{ todo.title }}</td>
            <td>{{ todo.description }}</td>
            <td>{{ todo.isActive ? 'True' : 'False' }}</td>
            <td>{{ todo.createDate }}</td>
            <td>
                <button @click="editTodo(todo)">Edit</button>
                <button @click="deleteTodo(todo.id)" style="margin-left: 2px;">Delete</button>
            </td>
        </tr>
    </tbody>
</table>
</template>

<style scoped>

</style>
